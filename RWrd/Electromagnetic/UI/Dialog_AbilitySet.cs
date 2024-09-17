using Electromagnetic.Abilities;
using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Dialog_AbilitySet : Window
    {
        public Dialog_AbilitySet(AbilitySet abilitySet, Pawn pawn) : base(null)
        {
            this.abilitySet = abilitySet;
            this.pawn = pawn;
            this.root = pawn.GetRoot();
            this.doCloseButton = false;
            this.doCloseX = true;
            this.forcePause = true;
            this.closeOnClickedOutside = true;
            this.routes = this.root.routes.ListFullCopy<RWrd_RouteDef>();
        }
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600f, 520f);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            inRect.yMax -= 50f;
            Text.Font = GameFont.Medium;
            Widgets.Label(inRect.TakeTopPart(40f).LeftHalf(), this.abilitySet.Name);
            int group = DragAndDropWidget.NewGroup(null);
            Rect rect7 = inRect.LeftHalf().ContractedBy(3f);
            rect7.xMax -= 8f;
            rect7.y += 50f;
            Widgets.DrawMenuSection(rect7);
            DragAndDropWidget.DropArea(group, rect7, delegate (object obj)
            {
                this.abilitySet.Abilities.Add((AbilityDef)obj);
            }, null);
            Vector2 vector = rect7.position + new Vector2(8f, 8f);
            using (List<AbilityDef>.Enumerator enumerator = this.abilitySet.Abilities.ToList<AbilityDef>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    AbilityDef def = enumerator.Current;
                    Rect rect2 = new Rect(vector, new Vector2(36f, 36f));
                    ITab_Pawn_RWrd.DrawAbility(rect2, def);
                    TooltipHandler.TipRegion(rect2, () => string.Format("{0}\n\n{1}\n\n{2}", def.LabelCap, def.description, "RWrd_ClickRemove".Translate().Resolve().ToUpper()), def.GetHashCode() + 2);
                    bool flag = Widgets.ButtonInvisible(rect2, true);
                    if (flag)
                    {
                        this.abilitySet.Abilities.Remove(def);
                    }
                    vector.x += 44f;
                    bool flag2 = vector.x + 36f >= rect7.xMax;
                    if (flag2)
                    {
                        vector.x = rect7.xMin + 8f;
                        vector.y += 44f;
                    }
                }
            }
            Rect inRect2 = inRect.RightHalf().ContractedBy(3f);
            Rect rect3 = inRect2.TakeTopPart(50f);
            Rect rect4 = rect3.TakeLeftPart(40f).ContractedBy(0f, 5f);
            Rect rect5 = rect3.TakeRightPart(40f).ContractedBy(0f, 5f);
            inRect2.y += 50f;
            bool flag3 = this.curIdx > 0 && Widgets.ButtonText(rect4, "<", true, true, true, null);
            if (flag3)
            {
                this.curIdx--;
            }
            bool flag4 = this.curIdx < this.routes.Count - 1 && Widgets.ButtonText(rect5, ">", true, true, true, null);
            if (flag4)
            {
                this.curIdx++;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3, string.Format("{0} / {1}", (this.routes.Count > 0) ? (this.curIdx + 1) : 0, this.routes.Count));
            Text.Anchor = TextAnchor.UpperLeft;
            bool flag5 = this.routes.Count > 0;
            if (flag5)
            {
                RWrd_RouteDef routeDef = this.routes[this.curIdx];
                ITab_Pawn_RWrd.DrawPathBackground(ref inRect2, routeDef);
                ITab_Pawn_RWrd.DoPathAbilities(inRect2, routeDef, this.abilityPos, delegate(Rect rect, AbilityDef def, RWrd_RouteNode routeNode)
                {
                    ITab_Pawn_RWrd.DrawAbility(rect, def);
                    bool flag = this.pawn.abilities.GetAbility(def) != null;
                    if (flag)
                    {
                        DragAndDropWidget.Draggable(group, rect, def, null, null);
                        TooltipHandler.TipRegion(rect, () => string.Format("{0}\n\n{1}", def.LabelCap, def.description), def.GetHashCode() + 1);
                    }
                    else
                    {
                        Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.6f), null);
                    }
                });
            }
            AbilityDef abilityDef = DragAndDropWidget.CurrentlyDraggedDraggable() as AbilityDef;
            bool flag6 = abilityDef != null;
            if (flag6)
            {
                ITab_Pawn_RWrd.DrawAbility(new Rect(Event.current.mousePosition, new Vector2(36f, 36f)), abilityDef);
            }
            Rect? rect6 = DragAndDropWidget.HoveringDropAreaRect(group, null);
            Rect valueOrDefault = new Rect();
            bool flag7;
            if (rect6 != null)
            {
                valueOrDefault = rect6.GetValueOrDefault();
                flag7 = true;
            }
            else
            {
                flag7 = false;
            }
            bool flag8 = flag7;
            if (flag8)
            {
                Widgets.DrawHighlight(valueOrDefault);
            }
        }
        private readonly Dictionary<AbilityDef, Vector2> abilityPos = new Dictionary<AbilityDef, Vector2>();
        private readonly Hediff_RWrd_PowerRoot root;
        private readonly AbilitySet abilitySet;
        private Pawn pawn;
        public List<RWrd_RouteDef> routes;
        private int curIdx;
    }
}
