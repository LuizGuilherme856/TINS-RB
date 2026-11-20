using UnityEngine;
using UnityEngine.UI;

namespace TINS.Campaign
{
    [ExecuteAlways]
    public sealed class CampaignSkillTreeConnectionView : MaskableGraphic
    {
        [SerializeField] private CampaignSkillTreeNodeView from;
        [SerializeField] private CampaignSkillTreeNodeView to;
        [SerializeField, Min(1f)] private float thickness = 4f;

        public void RefreshVisibility()
        {
            if (from == null || to == null) return;
            gameObject.SetActive(from.gameObject.activeSelf && to.gameObject.activeSelf);
            color = to.IsPurchased ? new Color32(104, 193, 171, 255) : new Color32(119, 70, 103, 255);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (from == null || to == null) return;
            Vector2 start = rectTransform.InverseTransformPoint(from.transform.position);
            Vector2 end = rectTransform.InverseTransformPoint(to.transform.position);
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * .5f);
            AddStrip(vertexHelper, start, end, normal * 1.65f, new Color32(42, 22, 48, 255));
            AddStrip(vertexHelper, start, end, normal, color);
        }

        private static void AddStrip(VertexHelper mesh, Vector2 start, Vector2 end, Vector2 normal, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(start - normal, tint, Vector2.zero);
            mesh.AddVert(start + normal, tint, Vector2.zero);
            mesh.AddVert(end + normal, tint, Vector2.zero);
            mesh.AddVert(end - normal, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }

        public void Configure(CampaignSkillTreeNodeView start, CampaignSkillTreeNodeView end)
        {
            from = start;
            to = end;
            raycastTarget = false;
            color = new Color(.50f, .06f, .20f, .75f);
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!Application.isPlaying) SetVerticesDirty();
        }
    }
}
