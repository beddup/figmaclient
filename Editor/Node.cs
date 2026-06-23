using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FigmaClient
{
    [Serializable]
    public class Node
    {
        public string id;
        public string name;
        public bool visible = true;
        public string type;

        public string componentId;
        public ComponentOverrides[] overrides;
        
        // public string blendMode; 
        public Node[] children;
        public AbsoluteBoundingBox absoluteBoundingBox; // done
        public Constraints constraints; // done
        public bool clipsContent;
        // public Fill[] background; 
        public Fill[] fills; 
        public Fill[] strokes; 
        public float strokeWeight; 
        public string strokeAlign; // （outside/inside/center ）
        public Effect[] effects; //  drop shadow, inner shadow, layer blur, background blur

        // public Color backgroundColor; 

        public Grid[] layoutGrids; 

        public string characters; // text 
        public Style style; // text style

        public bool HasChildren => children != null && children.Length > 0;


        public bool ShouldUseComponentImage()
        {
            if (string.IsNullOrEmpty(componentId)) return false;

            if (overrides == null || overrides.Length == 0) return true;
            
            // 如果改变了name, width, height 之外的属性，不能使用component image
            foreach (var overrideData in overrides) 
            {
                foreach(var overrideItem in overrideData.overriddenFields)
                {
                    if (!string.Equals(overrideItem, "name") &&
                        !string.Equals(overrideItem, "width") &&
                        !string.Equals(overrideItem, "height") &&
                        !string.Equals(overrideItem, "targetAspectRatio")
                        )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string IdForDownloadImage()
        {
            if (!ShouldUseComponentImage()) return id;
            return componentId;
        }
        
        public UnityEngine.Color GetSolidFillColor()
        {
            foreach (var fill in fills)
            {
                if (fill.visible && fill.renderType == Fill.FillRenderType.Color)
                {
                    return fill.color.ToColor();
                }
            }
            return UnityEngine.Color.clear;
        }

        public Node FindNode(string nodeId)
        {
            if (id == nodeId)
            {
                return this;
            }

            if (children != null)
            {
                foreach(var child in children)
                {
                    Node found = child.FindNode(nodeId);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        public List<Fill> GetValidFills()
        {
            return fills.Where(f => f.visible && f.opacity > 0).ToList();
        }

        public bool HasNonSolidColorOutlines()
        {
            return strokeWeight > 0 && strokes.Any(item => item.visible && item.opacity > 0 && item.type != "SOLID");
        }
        
        public Fill GetSolidColorOutline()
        {
            var outlines = GetValidSolidColorOutlines();
            if (outlines.Count > 0)
            {
                return outlines[0];
            }
            return null;
        }
        
        public List<Fill> GetValidSolidColorOutlines()
        {
            if (strokeWeight <= 0) return new List<Fill>();
            return strokes.Where(item => item.visible && item.opacity > 0 && item.type == "SOLID").ToList();
        }

        
        public Effect GetDropShadow()
        {
            var shadows = GetDropShadows();
            if (shadows.Count > 0) return shadows[0];
            return null;
        }
        
        public List<Effect> GetDropShadows()
        {
            return effects.Where(item => item.visible && 
                                               item.type == "DROP_SHADOW").ToList();
        }

        
        public Effect GetInnerShadow()
        {
            var shadows = GetInnerShadows();
            if (shadows.Count > 0) return shadows[0];
            return null;
        }

        public List<Effect> GetInnerShadows()
        {
            return effects.Where(item => item.visible && 
                                               item.type == "INNER_SHADOW").ToList();
        }
        
    }

    [Serializable]
    public class ComponentOverrides
    {
        public string id;
        public string[] overriddenFields;
    }

    [Serializable]
    public class AbsoluteBoundingBox
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }

        public Vector2 GetSize()
        {
            return new Vector2(width, height);
        }
    }

    [Serializable]
    public class Constraints
    {
        public string vertical;
        public string horizontal;
    }

    [Serializable]
    public class Fill
    {
        // https://help.figma.com/hc/en-us/articles/360041003694-Guide-to-fills
        // Fills can be solid colors, gradients, patterns, images, or videos (type)
        public float opacity = 1; 
        public string blendMode;
        public bool visible = true;
        public string type;
        public FigmaColor color;
        public string imageRef;
        public Vector[] gradientHandlePositions;
        public GradientStops[] gradientStops;
        
        
        public enum FillRenderType
        {
            Invisible,
            Image,
            Color,
            GRADIENT
        }

        public bool IsColorFill()
        {
            return type == "SOLID";
        }

        public FillRenderType renderType
        {
            get
            {
                if (!visible || opacity == 0) return FillRenderType.Invisible;
                if (type == "SOLID") return FillRenderType.Color;
                if (type == "GRADIENT_LINEAR") return FillRenderType.GRADIENT;
                return FillRenderType.Image;
            }
        }
        
        
        public UnityEngine.Color ToColor()
        {
            if (IsColorFill())
            {
                var c = color.ToColor();
                if (opacity != 1)
                {
                    c.a = opacity;
                }
                return c;
            }
            else
            {
                return new UnityEngine.Color(1,1,1, opacity);
            }
        }
    }
    [Serializable]
    public class FigmaColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public UnityEngine.Color ToColor()
        {
            return new UnityEngine.Color(r,g,b,a);
        }

        public string ToHexColor()
        {
            return ColorUtility.ToHtmlStringRGBA(ToColor());
        }
    }

    [Serializable]
    public class Grid
    {
        public string pattern;
        public float sectionSize;
        public bool visible = true;
        public FigmaColor color;
        public string alignment;
        public int gutterSize;
        public float offset;
        public int count;
    }
    
    [Serializable]
    public class Effect
    {
        public string type;
        public bool visible = true;
        public FigmaColor color;
        public string blendMode;
        public Vector offset;
        public float radius;
        public float spread;

    }
    
    [Serializable]
    public class Vector
    {
        public float x;
        public float y;

        public Vector2 ToVector2()
        {
            return new Vector2(x,y);
        }
        
    }

    [Serializable]
    public class GradientStops
    {
        public FigmaColor color;
        public float position;
    }

    [Serializable]
    public class Style
    {
        public string fontFamily;
        public string fontPostScriptName;
        public int fontWeight;
        public float fontSize;
        public string textAlignHorizontal;
        public string textAlignVertical;
        public float letterSpacing;
        public float lineHeightPx;
        public float lineHeightPercent;
        public string lineHeightUnit;
        public string textCase;
        public string textDecoration;
    }
    
}
