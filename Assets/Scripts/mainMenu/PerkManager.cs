using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerkManager
{
    [System.Serializable]
    public class PerkLevel
    {
        public int priceXP;
        public string description;
        public float value;
    }

    [System.Serializable]
    public class Perk
    {
        public PerkLevel[] levels;
        public string perkTitle;
        public string perkDescription;
        public string prefsKey;

        public PersistentPreferenceProxy<int> currentLevel;

        public PerkLevel GetCurrent()
        {
            return levels[currentLevel.value];
        }
    }

    [System.Serializable]
    public enum PerkClass
    {
        PC_Capture=0,
        PC_Fortify=1,
        PC_Stealth=2,
        PC_Stop=3
    }

    public static PersistentPreferenceProxy<int> TotalXP = new PersistentPreferenceProxy<int>("totalXP");
    public static PersistentPreferenceProxy<int> NextLevel = new PersistentPreferenceProxy<int>("nextLevel");
    public static PersistentPreferenceProxy<int> MaxLevel = new PersistentPreferenceProxy<int>("maxLevel");

    public static Perk[] storePerks = new Perk[]
        {
            new Perk()
            {
                prefsKey = "captureLVL",
                perkTitle = "CAPTURA",
                perkDescription = "Determina o tempo relativo da ação de captura",
                currentLevel = new PersistentPreferenceProxy<int>("captureLVL"),
                levels = new PerkLevel[]
                    {
                        new PerkLevel()
                        {
                            priceXP = 0,
                            value = 1f,
                            description = "LVL 1: <b>100%</b> do tempo base do nó"
                        },
                        new PerkLevel()
                        {
                            priceXP = 1000,
                            value = 0.9f,
                            description = "LVL 2: O tempo de captura fica reduzido para <b>90%</b> do tempo base do nó"
                        },
                        new PerkLevel()
                        {
                            priceXP = 2500,
                            value = 0.8f,
                            description = "LVL 3: Captura o nó em 80% do seu tempo base"
                        },
                        new PerkLevel()
                        {
                            priceXP = 5000,
                            value = 0.7f,
                            description = "LVL 4: A captura do nó leva 70% do tempo base"
                        },
                        new PerkLevel()
                        {
                            priceXP = 10000,
                            value = 0.6f,
                            description = "LVL 5: O tempo de captura fica reduzido para 60% do tempo base do nó"
                        }
                    }
            },

            new Perk()
            {
                prefsKey = "fortifyLVL",
                perkTitle = "FORTIFICAÇÃO",
                perkDescription = "Determina a resistência da fortificação",
                currentLevel = new PersistentPreferenceProxy<int>("fortifyLVL"),
                levels = new PerkLevel[]
                    {
                        new PerkLevel()
                        {
                            priceXP = 0,
                            value = 1f,
                            description = "LVL 1: <b>100%</b> da resistência base da fortificação"
                        },
                        new PerkLevel()
                        {
                            priceXP = 500,
                            value = 1.2f,
                            description = "LVL 2: A resistência da fortificação fica <b>20%</b> maior"
                        },
                        new PerkLevel()
                        {
                            priceXP = 1000,
                            value = 1.35f,
                            description = "LVL 3: A resistência da fortificação fica <b>35%</b> maior"
                        },
                        new PerkLevel()
                        {
                            priceXP = 2500,
                            value = 1.5f,
                            description = "LVL 4: Amplia em <b>50%</b> a resistência da fortificação em relação ao valor base"
                        },
                        new PerkLevel()
                        {
                            priceXP = 7500,
                            value = 2f,
                            description = "LVL 5: <b>Dobra</b> a resistência das fortificações"
                        }
                    }
            },

            new Perk()
            {
                prefsKey = "stealthLVL",
                perkTitle = "DETECÇÃO",
                perkDescription = "Influencia a chance de uma ação do jogador ser detectada",
                currentLevel = new PersistentPreferenceProxy<int>("stealthLVL"),
                levels = new PerkLevel[]
                    {
                        new PerkLevel()
                        {
                            priceXP = 0,
                            value = 1f,
                            description = "LVL 1: Chance base de detecção inalterada"
                        },
                        new PerkLevel()
                        {
                            priceXP = 900,
                            value = 0.95f,
                            description = "LVL 2: A chance de detecção é reduzida em <b>5%</b>"
                        },
                        new PerkLevel()
                        {
                            priceXP = 2700,
                            value = 0.9f,
                            description = "LVL 3: Reduz a possibilidade de detecção em <b>10%</b>"
                        },
                        new PerkLevel()
                        {
                            priceXP = 6000,
                            value = 0.85f,
                            description = "LVL 4: Aprimora em <b>15%</b> a furtividade do jogador"
                        },
                        new PerkLevel()
                        {
                            priceXP = 15000,
                            value = 0.8f,
                            description = "LVL 5: Chance base de detecção 20% reduzida"
                        }
                    }
            },

            new Perk()
            {
                prefsKey = "stopwormLVL",
                perkTitle = "STOP!",
                perkDescription = "Regula a duração do <b>STOP! WORM</b> mediante seu uso",
                currentLevel = new PersistentPreferenceProxy<int>("stopwormLVL"),
                levels = new PerkLevel[]
                    {
                        new PerkLevel()
                        {
                            priceXP = 0,
                            value = 3f,
                            description = "LVL 1: Interrompe a propagação do <b>firewall</b> por <b>3</b> segundos"
                        },
                        new PerkLevel()
                        {
                            priceXP = 3000,
                            value = 4f,
                            description = "LVL 2: O efeito passa a durar <b>4</b> segundos"
                        },
                        new PerkLevel()
                        {
                            priceXP = 6000,
                            value = 5f,
                            description = "LVL 3: A propagação do <b>programa de rastreamento</b> é bloqueada por <b>5</b> segundos"
                        },
                        new PerkLevel()
                        {
                            priceXP = 10000,
                            value = 6f,
                            description = "LVL 4: O <b>firewall</b> sofre um retardo de <b>6</b> segundos"
                        },
                        new PerkLevel()
                        {
                            priceXP = 15000,
                            value = 7f,
                            description = "LVL 5: Atraso de <b>7</b> segundos no <b>programa de rastreamento</b>"
                        }
                    }
            }
        };
    
	
    public static void ResetAllPrefsToZero()
    {
        TotalXP.value = 0;
        NextLevel.value = 1;
        MaxLevel.value = 1;
        foreach (var p in storePerks)
            p.currentLevel.value = 0;
    }
}
