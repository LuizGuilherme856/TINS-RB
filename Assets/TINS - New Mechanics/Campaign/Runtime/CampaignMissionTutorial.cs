using TMPro;
using UnityEngine;

namespace TINS.Campaign
{
    public sealed class CampaignMissionTutorial : MonoBehaviour
    {
        private enum Lesson { Move, Fire, Collect, Deliver, Explore }

        [SerializeField] private TMP_Text heading;
        [SerializeField] private TMP_Text instruction;

        private PlayerInputHandler player;
        private GunAmmo ammo;
        private CampaignMissionRuntime mission;
        private PurificationReveal light;
        private Vector2 initialPosition;
        private float initialRadius;
        private int initialAmmo;
        private Lesson lesson;
        private float dismissAt;

        internal void Begin(CampaignMissionRuntime owner, PlayerInputHandler input, PurificationReveal reveal)
        {
            mission = owner;
            player = input;
            light = reveal;
            if (player == null) return;
            initialPosition = player.transform.position;
            initialRadius = light.Radius;
            ammo = player.GetComponentInChildren<GunManager>()?.Ammo;
            initialAmmo = ammo?.Rounds ?? 0;
            lesson = Lesson.Move;
            gameObject.SetActive(true);
            Redraw();
        }

        private void OnEnable()
        {
            if (player != null) player.OnControllerChange += Redraw;
        }

        private void OnDisable()
        {
            if (player != null) player.OnControllerChange -= Redraw;
        }

        private void Update()
        {
            if (mission == null || mission.IsResolved)
            {
                gameObject.SetActive(false);
                return;
            }
            Lesson next = lesson;
            if (light.Radius > initialRadius + .01f) next = Lesson.Explore;
            else if (mission.SoulsCollected > 0) next = Lesson.Deliver;
            else if (ammo != null && ammo.Rounds < initialAmmo) next = Lesson.Collect;
            else if (lesson == Lesson.Move && ((Vector2)player.transform.position - initialPosition).sqrMagnitude >= .36f)
                next = Lesson.Fire;

            if (next > lesson)
            {
                lesson = next;
                Redraw();
                if (lesson == Lesson.Explore) dismissAt = Time.time + 8f;
            }
            if (lesson == Lesson.Explore && Time.time >= dismissAt) gameObject.SetActive(false);
        }

        private void Redraw()
        {
            if (player == null) return;
            bool pad = player.isUsingController;
            switch (lesson)
            {
                case Lesson.Move:
                    heading.text = "MOVA-SE";
                    instruction.text = pad ? "Use o analógico esquerdo para se mover." : "Use WASD para se mover.";
                    break;
                case Lesson.Fire:
                    heading.text = "DEFENDA O CENTRO";
                    instruction.text = pad ? "Mire com o analógico direito e dispare com o gatilho direito."
                        : "Mire com o mouse e dispare com o botão esquerdo.";
                    break;
                case Lesson.Collect:
                    heading.text = "RECOLHA ALMAS";
                    instruction.text = "Aproxime-se das almas deixadas pelos inimigos ao cair.";
                    break;
                case Lesson.Deliver:
                    heading.text = "ALIMENTE A LUZ";
                    instruction.text = "Volte ao centro. As almas entram sozinhas e ampliam a área iluminada.";
                    break;
                case Lesson.Explore:
                    heading.text = "EXPANDA A ÁREA ILUMINADA";
                    instruction.text = "Explore a penumbra atrás de recursos. A luz do centro desacelera os inimigos.";
                    break;
            }
        }
    }
}
