#region
using System;
using System.Collections;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace SF_Template
{


    internal class program
    {

        private const string Champion = "Amumu"; 

        private static Orbwalking.Orbwalker Orbwalker; 

        private static Spell Q;
        private static Spell W; 
        private static Spell E; 
        private static Spell R;

        private static List<Spell> SpellList = new List<Spell>(); 

        private static Menu Config; 

        private static Items.Item DFG;
        private static Items.Item RDO;

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } } 


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad; 

        }


        static void Game_OnGameLoad(EventArgs args)
        {

            if (ObjectManager.Player.BaseSkinName != Champion) return;

            Q = new Spell(SpellSlot.Q, 1100); 
            W = new Spell(SpellSlot.W, 300); 
            E = new Spell(SpellSlot.E, 350); 
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.5f, 80, 2000, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            DFG = new Items.Item(3128, 490f);
            RDO = new Items.Item(3143, 500f);

            //Creating a menu
            Config = new Menu("justAmumu", "String_Name", true);

            //Ts 
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalk
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo", "Combo")); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);  
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
  
            //Jungle Clear
            Config.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleQ", "Use Q")).SetValue(true);
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleW", "Use W")).SetValue(true);
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleE", "Use E")).SetValue(true);
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleKey", "Bind").SetValue(new KeyBind(86, KeyBindType.Press)));
            
            //Jungle Clear
            Config.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("ClearQ", "Use Q")).SetValue(true);
            Config.SubMenu("LaneClear").AddItem(new MenuItem("ClearW", "Use W")).SetValue(true);
            Config.SubMenu("LaneClear").AddItem(new MenuItem("ClearE", "Use E")).SetValue(true);
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneKey", "Bind").SetValue(new KeyBind(86, KeyBindType.Press)));
            //KS
            Config.AddSubMenu(new Menu("KS", "KS"));
            Config.SubMenu("KS").AddItem(new MenuItem("KSW", "KS with Q")).SetValue(true);
            Config.SubMenu("KS").AddItem(new MenuItem("KSE", "KS with E")).SetValue(true);

            //Packet casting
            Config.AddSubMenu(new Menu("Packet Cast", "Packet"));
            Config.SubMenu("Packet").AddItem(new MenuItem("PUse", "Use ?")).SetValue(true);

            //Items
            Config.AddSubMenu(new Menu("Items", "Items"));
            Config.SubMenu("Items").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            Config.SubMenu("Items").AddItem(new MenuItem("DFGu", "DFG")).SetValue(true);
            Config.SubMenu("Items").AddItem(new MenuItem("RDOu", "RDO")).SetValue(true); 

            //Range Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawEnable", "Enable Drawing"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);


            Config.AddToMainMenu();
            Game.PrintChat("justAmumu Loaded # MixX");
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

    
        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("JungleKey").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (Config.Item("LaneKey").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active) 
            {
                Combo(); 
            }

            if (Config.Item("KSW").GetValue<bool>() || Config.Item("KSE").GetValue<bool>()) 
            {
                KSW();
            }

        }
        
        static void Combo()
        {


            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return; 

            //Combo
            if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
            {
                var prediction = Q.GetPrediction(target);
                if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                {
                    Q.Cast(prediction.CastPosition, Config.Item("PUse").GetValue<bool>());

                }
            }
            if (target.IsValidTarget(W.Range) && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.CountEnemysInRange((int)W.Range) >= 1 && (Config.Item("UseWCombo").GetValue<bool>()))
            {
                if (Player.Distance(target) <= W.Range)
                {
                    W.Cast(Player, Config.Item("PUse").GetValue<bool>());
                }
            }
            if (target.IsValidTarget(E.Range) && W.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {
                E.Cast(target, Config.Item("PUse").GetValue<bool>(), true);
            }
            if (target.IsValidTarget(R.Range) && W.IsReady() && (Config.Item("UseRCombo").GetValue<bool>()))
            {   
                R.Cast(target, Config.Item("PUse").GetValue<bool>() , true); 
            }

            //Items using
            if (Config.Item("UseItems").GetValue<bool>()) 
            {
                if (Config.Item("DFGu").GetValue<bool>())
                {
                    if (Player.Distance(target) <= DFG.Range)
                    {
                        DFG.Cast(target);
                    }
                }
                if (Config.Item("RDOu").GetValue<bool>())
                {
                    if (Player.Distance(target) <= RDO.Range) 
                    {
                        RDO.Cast();
                    }
                }
            }



        }

        //Jungle Clear
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
            if (!mobs.Any()) return;

            var mob = mobs.First();

            if (Config.Item("JungleQ").GetValue<bool>())
            {
                var prediction = Q.GetPrediction(mob);
                if (prediction.Hitchance >= HitChance.VeryHigh)
                {
                    Q.Cast(prediction.CastPosition, Config.Item("PUse").GetValue<bool>());

                }
            }
            if (Config.Item("JungleW").GetValue<bool>())
            {
                if (Player.Distance(mob) <= W.Range)
                {
                    W.Cast(Player, Config.Item("PUse").GetValue<bool>());
                }
            }
            if (Config.Item("JungleE").GetValue<bool>())
            {
                E.Cast(mob, Config.Item("PUse").GetValue<bool>());
            }
        }
        private static void LaneClear()
        {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                if (!mobs.Any()) return;

                var mob = mobs.First();

                if (Config.Item("ClearQ").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(mob);
                    if (prediction.Hitchance >= HitChance.VeryHigh)
                    {
                        Q.Cast(prediction.CastPosition, Config.Item("PUse").GetValue<bool>());

                    }
                }
                if (Config.Item("ClearW").GetValue<bool>())
                {
                    if (Player.Distance(mob) <= W.Range)
                    {
                        W.Cast(Player, Config.Item("PUse").GetValue<bool>());
                    }
                }
                if (Config.Item("ClearE").GetValue<bool>())
                {
                    E.Cast(mob, Config.Item("PUse").GetValue<bool>());
                }
        }


        //KS System
        private static void KSW() 
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            var prediction = Q.GetPrediction(target);

            if (E.IsReady() && Config.Item("KSE").GetValue<bool>())
            {
                if (target.Health < GetEDamage(target))
                {
                    E.Cast(target, Config.Item("PUse").GetValue<bool>());
                }
            }

            if (Q.IsReady() && Config.Item("KSW").GetValue<bool>()) 
            {
                if (target.Health < GetQDamage(target)) //If target's hp is lower then the damage of spell W. Its calling the function GetWDamge here which can be found below
                {
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1)
                    {
                        Q.Cast(prediction.CastPosition, Config.Item("PUse").GetValue<bool>());
                    }
                }
            }
        }


        //Damage
        private static float GetQDamage(Obj_AI_Base enemy) 
        {
            double damage = 0d;

            if (Q.IsReady()) 
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            return (float)damage; 
        }

        private static float GetEDamage(Obj_AI_Base enemy) 
        {
            double damage = 0d; 

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float)damage; 
        }


        //Drwaings
        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Red);
            }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Green);
            }
            if (Config.Item("DrawE").GetValue<bool>())
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Blue);
            }
        }





       }
}


    
