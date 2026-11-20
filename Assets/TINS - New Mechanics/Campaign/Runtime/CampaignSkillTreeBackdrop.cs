using UnityEngine;
using UnityEngine.UI;

namespace TINS.Campaign
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CampaignSkillTreeBackdrop : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect area = rectTransform.rect;
            const float spacing = 32f;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            for (float y = area.yMin + spacing * .5f; y < area.yMax - 2f; y += spacing)
            {
                for (float x = area.xMin + spacing * .5f; x < area.xMax - 2f; x += spacing)
                {
                    int first = mesh.currentVertCount;
                    vertex.position = new Vector2(x, y);
                    mesh.AddVert(vertex);
                    vertex.position = new Vector2(x + 2f, y);
                    mesh.AddVert(vertex);
                    vertex.position = new Vector2(x + 2f, y + 2f);
                    mesh.AddVert(vertex);
                    vertex.position = new Vector2(x, y + 2f);
                    mesh.AddVert(vertex);
                    mesh.AddTriangle(first, first + 1, first + 2);
                    mesh.AddTriangle(first, first + 2, first + 3);
                }
            }
        }
    }
}
