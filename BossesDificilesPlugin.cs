using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BossesDificiles
{
    [ApiVersion(2, 1)]
    public class BossesDificilesPlugin : TerrariaPlugin
    {
        public override string Name => "BossesDificiles";
        public override string Author => "Servidor";
        public override string Description => "Bosses con más vida, más esbirros y ataques más rápidos (configurable por JSON).";
        public override Version Version => new Version(1, 0, 0);

        private BossConfig config;
        private bool dentro; // evita recursión al llamar npc.AI()
        private readonly Dictionary<int, int> ticks = new();
        private readonly Dictionary<int, float> acum = new();

        public BossesDificilesPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            config = BossConfig.Cargar();
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            ServerApi.Hooks.NpcAIUpdate.Register(this, OnNpcAI);
            GeneralHooks.ReloadEvent += OnReload;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNpcAI);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        private void OnReload(ReloadEventArgs e)
        {
            config = BossConfig.Cargar();
            e.Player?.SendSuccessMessage("[BossesDificiles] Configuración recargada.");
        }

        private bool EsBoss(NPC npc)
        {
            return npc.boss || config.Bosses.ContainsKey(npc.FullName);
        }

        // Vida multiplicada al aparecer
        private void OnNpcSpawn(NpcSpawnEventArgs e)
        {
            var npc = Main.npc[e.NpcId];

            // Reinicia el estado por si el índice se reutiliza
            ticks.Remove(e.NpcId);
            acum.Remove(e.NpcId);

            if (!EsBoss(npc)) return;

            float mult = config.Bosses.TryGetValue(npc.FullName, out var m)
                ? m
                : config.MultiplicadorPorDefecto;

            npc.lifeMax = (int)(npc.lifeMax * mult);
            npc.life = npc.lifeMax;
            NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npc.whoAmI);
        }

        // Ataques más rápidos y esbirros extra
        private void OnNpcAI(NpcAiUpdateEventArgs e)
        {
            if (dentro) return;

            var npc = e.Npc;
            if (npc == null || !npc.active || !EsBoss(npc)) return;

            // Ataque más rápido: ejecuta la IA extra
            float velocidad = Math.Clamp(config.VelocidadAtaque, 1f, 5f);
            float a = acum.GetValueOrDefault(npc.whoAmI) + (velocidad - 1f);
            dentro = true;
            try
            {
                while (a >= 1f)
                {
                    npc.AI();
                    a -= 1f;
                }
            }
            finally
            {
                dentro = false;
            }
            acum[npc.whoAmI] = a;

            // Esbirros extra (solo el boss principal)
            if (!npc.boss) return;

            int t = ticks.GetValueOrDefault(npc.whoAmI) + 1;
            ticks[npc.whoAmI] = t;
            if (t < config.SegundosEntreOleadas * 60) return;
            ticks[npc.whoAmI] = 0;

            if (!config.Esbirros.TryGetValue(npc.FullName, out int tipo)) return;

            int vivos = Main.npc.Count(n => n.active && n.type == tipo);
            for (int i = 0; i < config.EsbirrosPorOleada && vivos + i < config.MaxEsbirros; i++)
            {
                NPC.NewNPC(npc.GetSpawnSourceForNPCFromNPCAI(),
                    (int)npc.Center.X, (int)npc.Center.Y, tipo);
            }
        }
    }
}
