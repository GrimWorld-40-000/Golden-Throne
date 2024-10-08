using System;
using UnityEngine;
using Verse;

namespace GoldenThrone.Buildings
{
    public class GoldenThroneModuleGizmo(Building_GoldenThrone throne) : Gizmo
    {
        protected Building_GoldenThrone Throne = throne;

        private static Color _bg = new Color(0.6215686275f, 0.4882352941f, 0);
        private static Color _fg = new Color(0.9215686275f, 0.7882352941f, 0.1294117647f);
        private static Color _disabledBg = new Color(0.7f, 0.2f, 0);
        private static Color _disabledFg = new Color(1f, 0.3f, 0.1294117647f);

        private Color BackgroundColor => Throne.IsEnabled ? _bg : _disabledBg;
        private Color ForegroundColor => Throne.IsEnabled ? _fg : _disabledFg;

        private static float GetTickX(Rect bounds, int index)
        {
            return bounds.x + bounds.width / Building_GoldenThrone.MaxModuleCapacity * index;
        }

        private const float Width = 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Text.Font = GameFont.Tiny;
            Rect rect1 = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect1);
            Rect fillArea = rect1.ContractedBy(6f);
            float num = fillArea.height * 2 / 3f;
            Rect barRegion = new Rect(fillArea.x, fillArea.y, fillArea.width, num);
            Rect labelRegion = new Rect(fillArea.x, fillArea.y + num, fillArea.width, num + 2f);

            DrawBar(barRegion);
            Widgets.Label(labelRegion, Throne.ModuleCapacityReport);


            Text.Font = GameFont.Small;
            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawBar(Rect outerBounds)
        {
            Rect rect = outerBounds.ContractedBy(2);
            
            if (rect.height > 70.0)
            {
                float num = (float)((rect.height - 70.0) / 2.0);
                rect.height = 70f;
                rect.y += num;
            }

            Widgets.DrawRectFast(rect, BackgroundColor);
            
            Widgets.DrawRectFast(new Rect(rect.x, rect.y, GetTickX(rect, Throne.TotalCapacityUsed) - rect.x, rect.height), ForegroundColor);

            float topY = rect.y + 3;
            float bottomY = rect.y + rect.height;
            
            for (int index = 1; (double)index < Building_GoldenThrone.MaxModuleCapacity; ++index)
            {
                float x = GetTickX(rect, index);
                Widgets.DrawLine(new Vector2(x, topY), new Vector2(x, bottomY), new Color(0.2f, 0.2f, 0.2f), 1);
            }
            
            
            Color color = GUI.color;
            GUI.color = Color.black;
            Widgets.DrawBox(outerBounds, 4);
            GUI.color = color;
        }
        

        public override float GetWidth(float maxWidth) => Width;
    }
}