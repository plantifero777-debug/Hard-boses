using Newtonsoft.Json;
using Terraria.ID;
using TShockAPI;

namespace BossesDificiles
{
    public class BossConfig
    {
        // Vida: multiplicador para cualquier boss que no esté en "Bosses"
        public float MultiplicadorPorDefecto = 5f;

        // Ataque más rápido: 2 = el doble de rápido, 1 = normal
        public float VelocidadAtaque = 2f;

        // Esbirros extra
        public int SegundosEntreOleadas = 8;
        public int EsbirrosPorOleada = 3;
        public int MaxEsbirros = 12;

        // Multiplicador de vida por boss (nombre en inglés tal como sale en el juego)
        public Dictionary<string, float> Bosses = new()
        {
            // Pre-Hardmode
            { "King Slime", 5f },
            { "Eye of Cthulhu", 5f },
            { "Eater of Worlds", 5f },
            { "Brain of Cthulhu", 5f },
            { "Queen Bee", 5f },
            { "Skeletron", 5f },
            { "Deerclops", 5f },
            { "Wall of Flesh", 5f },

            // Hardmode
            { "Queen Slime", 5f },
            { "Retinazer", 5f },
            { "Spazmatism", 5f },
            { "The Destroyer", 5f },
            { "Skeletron Prime", 5f },
            { "Plantera", 5f },
            { "Golem", 5f },
            { "Duke Fishron", 5f },
            { "Empress of Light", 5f },
            { "Lunatic Cultist", 5f },
            { "Moon Lord", 5f }
        };

        // Tipo de esbirro (ID de NPC) que invoca cada boss. Los que no estén aquí no invocan.
        public Dictionary<string, int> Esbirros = new()
        {
            { "King Slime", NPCID.BlueSlime },
            { "Eye of Cthulhu", NPCID.ServantofCthulhu },
            { "Brain of Cthulhu", NPCID.Creeper },
            { "Queen Bee", NPCID.Bee },
            { "Skeletron", NPCID.CursedSkull },
            { "The Destroyer", NPCID.Probe }
        };

        private static string Ruta => Path.Combine(TShock.SavePath, "bosses-dificiles.json");

        public static BossConfig Cargar()
        {
            try
            {
                if (!File.Exists(Ruta))
                {
                    var nueva = new BossConfig();
                    File.WriteAllText(Ruta, JsonConvert.SerializeObject(nueva, Formatting.Indented));
                    return nueva;
                }

                // Replace: los diccionarios del JSON sustituyen a los de por defecto
                var ajustes = new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                var cfg = JsonConvert.DeserializeObject<BossConfig>(File.ReadAllText(Ruta), ajustes);
                return cfg ?? new BossConfig();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[BossesDificiles] Error leyendo el JSON, usando valores por defecto: {ex.Message}");
                return new BossConfig();
            }
        }
    }
}
