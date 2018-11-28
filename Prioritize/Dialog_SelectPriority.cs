using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace Prioritize
{
    public class Dialog_SelectPriority : Window
    {
        public Dialog_SelectPriority()
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(140, 100);
        public override void DoWindowContents(Rect inRect)
        {
            string text = Widgets.TextField(new Rect(0, 15, inRect.width, 35f), MainMod.SelectedPriority.ToString());
            if (short.TryParse(text, out short res)) MainMod.SelectedPriority = res;
            else
            {
                text = MainMod.SelectedPriority.ToString();
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
            }
        }
    }
}
