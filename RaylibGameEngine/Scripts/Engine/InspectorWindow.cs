//using System;
//using System.Collections.Generic;
//using Levels;
//using Raylib_cs;
//using MathExtras;
//using System.Diagnostics.CodeAnalysis;

//namespace Engine
//{
//    public static class EditorGUI
//    {
//        //Colours
//        public static Color windowBgColor = new Color(220, 220, 220, 255);
//        public static Color windowBorderColor = new Color(150, 150, 150, 255);

//        public static Color buttonBgColor = new Color(240, 240, 240, 255);
//        public static Color buttonLineColor = new Color(100, 100, 100, 255);

//        //Window class
//        public class Window
//        {
//            public ScreenAnchor anchor;
//            public Vector2Int offset;

//            public int X => anchor.X + offset.X;
//            public int Y => anchor.Y + offset.Y;
//            public int width, height;

//            public bool isVisible = true;

//            public virtual void Draw()
//            {
//                Raylib.DrawRectangle(X, Y, width, height, windowBgColor);
//                Raylib.DrawRectangleLinesEx(new Rectangle(X, Y, width, height), 2, windowBorderColor);
//            }

//            public virtual bool IsMouseOver()
//            {
//                if (!isVisible)
//                {
//                    return false;
//                }

//                Vector2Int mousePos = Raylib.GetMousePosition().ToVector2Int();
//                return mousePos.X > X && mousePos.Y > Y && mousePos.X - X < width && mousePos.Y - Y < height;
//            }
//        }

//        public struct ScreenAnchor
//        {
//            public int X;
//            public int Y;

//            public ScreenAnchor(int x, int y)
//            {
//                this.X = x;
//                this.Y = y;
//            }

//            public static ScreenAnchor TopLeft => new ScreenAnchor(0, 0);
//            public static ScreenAnchor TopRight => new ScreenAnchor(Screen.screenWidth, 0);
//            public static ScreenAnchor BottomLeft => new ScreenAnchor(0, Screen.screenHeight);
//            public static ScreenAnchor BottomRight => new ScreenAnchor(Screen.screenWidth, Screen.screenHeight);
//        }
//    }

//    public static class Inspector
//    {
//        public class InspectorWindow : EditorGUI.Window
//        {
//            public InspectorValues inspector = new InspectorValues(Semisolid.GetInspectorProfile());

//            public Vector2Int padding = new Vector2Int(20, 15);
//            public int CalculateWindowHeight => (padding.Y * 2) + ((inspector.profile.fields.Count + 1) * 30);

//            public override void Draw()
//            {
//                height = CalculateWindowHeight;
//                base.Draw();

//                new Semisolid(10, 10, 2, 5, true, 0) { culling = new Semisolid.CullingRule(true, true, true, true) }.SetInspectorValues(ref inspector);


//                Raylib.DrawText(inspector.profile.type.Name + ":", X + padding.X, Y + padding.Y + 2, 20, EditorGUI.buttonLineColor);
//                for (int i = 0; i < inspector.profile.fields.Count; i++)
//                {
//                    inspector.profile.fields[i].Draw(X + padding.X, Y + padding.Y + 2 + ((i+1) * 30), width - ((padding.X * 2)+5), inspector.values[i]);
//                }
//            }
//        }

//        //Interface for objects that can be inspected
//        public interface IInspectable
//        {
//            public static InspectorProfile GetInspectorProfile() => new InspectorProfile();
//            public void SetInspectorValues(ref InspectorValues values);
//            public void ApplyInspectorValues(InspectorValues values);
//        }

//        //Class to save values modified in an inspector window
//        public class InspectorValues
//        {
//            //Inspector profile for fields
//            public InspectorProfile profile;

//            //List of values
//            public readonly List<object> values = new List<object>();

//            //Sets an inspector field's value
//            public void SetValue(string id, object value)
//            {
//                while (values.Count < profile.fields.Count)
//                {
//                    values.Add(new object());
//                }

//                for (int i = 0; i < profile.fields.Count; i++)
//                {
//                    if (profile.fields[i].name == id)
//                    {
//                        values[i] = value;
//                        return;
//                    }
//                }
//            }

//            //Gets a value from an inspector field
//            public T GetValue<T>(string id)
//            {
//                for (int i = 0; i < profile.fields.Count; i++)
//                {
//                    if (profile.fields[i].name == id)
//                    {
//                        return (T)values[i];
//                    }
//                }
//                return default;
//            }

//            public InspectorValues(InspectorProfile profile)
//            {
//                this.profile = profile;
//                values = new List<object>(profile.fields.Count);
//            }
//        }

//        //Profile of value types for an object
//        public struct InspectorProfile
//        {
//            //Inspected object type
//            public readonly Type type;

//            //List of fields
//            public readonly List<InspectorField> fields;

//            //Initialisation
//            public InspectorProfile(Type type, List<InspectorField> list)
//            {
//                this.type = type;
//                fields = list;
//            }
//        }

//        //Single field in the inspector
//        public struct InspectorField
//        {
//            //Variables
//            public string name;
//            public Type type;
//            public bool readOnly;

//            public void Draw(int x, int y, int width, object value)
//            {
//                Raylib.DrawText(name, x, y+5, 20, EditorGUI.buttonLineColor);

//                if (type == typeof(bool))
//                {
//                    Raylib.DrawRectangle(x + width - 20, y, 25, 25, EditorGUI.buttonBgColor);
//                    Raylib.DrawRectangleLinesEx(new Rectangle(x + width - 20, y , 25, 25), 2, EditorGUI.windowBorderColor);

//                    if ((bool)value)
//                    {
//                        Raylib.DrawRectangle(x + width - 20 + 5, y + 5, 15, 15, EditorGUI.buttonLineColor);
//                    }
//                }
//                else if (type == typeof(int))
//                {
//                    Raylib.DrawRectangle(x + width - 80, y, 85, 25, EditorGUI.buttonBgColor);
//                    Raylib.DrawRectangleLinesEx(new Rectangle(x + width - 80, y, 85, 25), 2, EditorGUI.windowBorderColor);

//                    Raylib.DrawText(((int)value).ToString(), x + width - 3 - Raylib.MeasureText(((int)value).ToString(), 20), y + 3, 20, EditorGUI.buttonLineColor);
//                }
//                else if (type == typeof(string))
//                {
//                    Raylib.DrawRectangle(x + width - 120, y, 125, 25, EditorGUI.buttonBgColor);
//                    Raylib.DrawRectangleLinesEx(new Rectangle(x + width - 120, y, 125, 25), 2, EditorGUI.windowBgColor);

//                    Raylib.DrawText(((int)value).ToString(), x + width - 3 - Raylib.MeasureText(((int)value).ToString(), 20), y + 3, 20, EditorGUI.buttonLineColor);
//                }
//            }

//            //Initialisation
//            public InspectorField(Type t, string id, bool readOnly = false)
//            {
//                name = id;
//                type = t;
//                this.readOnly = readOnly;
//            }
//        }
//    }
//}