// 
// HLSLNode.cs
// 
// 
// Created by LunarEclipse on 2023-07-19 11:35.
// Copyright © 2023 LunarEclipse. All rights reserved.

#if UNITY_EDITOR && USE_SHADER_GRAPH

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphing;
using UnityEditor.Rendering;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Drawing.Inspector;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.UIElements;

// [TODO] Shader not working when using HLSL function

namespace Luna.Extensions.ShaderGraph
{
    // [Title("Utility", "HLSL Function")]
    class HLSLNode : CustomFunctionNode
    {
        public HLSLNode()
        {
            name = "HLSL Function";
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            UpdateNodeAfterDeserialization();
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            switch (sourceType)
            {
                case HlslSourceType.File:
                    string path = AssetDatabase.GUIDToAssetPath(functionSource);

                    // This is required for upgrading without console errors
                    if (!string.IsNullOrEmpty(path))
                    {
                        string fileContent = File.ReadAllText(path);
                        UpdateNode(fileContent);
                    }

                    break;
                case HlslSourceType.String:
                    if (!string.IsNullOrEmpty(functionBody))
                        UpdateNode(functionBody);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateNodeName();
        }

        void UpdateNode(string hlsl)
        {
            Debug.Log($"{hlsl}");
            RemoveSlotsNameNotMatching(new List<int>());

            // Define the regular expressions to extract parameter types and 'out' parameters
            Regex functionRegex = new Regex(@"void\s+(\w+)_float\s*\(\s*(.*?)\s*\)", RegexOptions.Singleline);
            Regex parameterRegex = new Regex(@"(?:in\s+|out\s+)?(\w+)\s+(\w+)", RegexOptions.Singleline);

            // Find the function definition
            List<MaterialSlot> slots = new();
            bool hasOut = false;

            MatchCollection functionMatchs = functionRegex.Matches(hlsl);
            foreach (Match functionMatch in functionMatchs)
            {
                string functionName = functionMatch.Groups[1].Value;
                string parameterList = functionMatch.Groups[2].Value;

                this.functionName = functionName;

                Debug.Log($"Function name: {functionName}");
                Debug.Log("Parameter types:");

                // Find and process each parameter in the parameter list
                MatchCollection parameterMatches = parameterRegex.Matches(parameterList);
                for (int i = 0; i < parameterMatches.Count; i++)
                {
                    var parameterMatch = parameterMatches[i];

                    string parameterType = parameterMatch.Groups[1].Value;
                    string parameterName = parameterMatch.Groups[2].Value;

                    Debug.Log($"- {parameterType} {parameterName}");

                    slots.Add(MaterialSlot.CreateMaterialSlot(
                            ConvertTypeToSlotValueType(parameterType),
                            i,
                            parameterName,
                            parameterName,
                            parameterMatch.Value.Contains("out") ? SlotType.Output : SlotType.Input,
                            Vector4.zero));

                    // Check if the parameter has the 'out' keyword
                    if (parameterMatch.Value.Contains("out"))
                    {
                        Debug.Log($"    {parameterName} has 'out' keyword.");
                        hasOut = true;
                    }
                }
            }

            RemoveSlotsNameNotMatching(slots.Select(x => x.id));

            if (!hasOut) return;

            foreach (var slot in slots)
            {
                AddSlot(slot);
            }
        }

        void UpdateNodeName()
        {
            if ((functionName == defaultFunctionName) || (functionName == null))
                name = "HLSL Function";
            else
                name = functionName + " (HLSL Function)";
        }

        private static SlotValueType ConvertTypeToSlotValueType(string p)
        {
            if (p == "bool")
                return SlotValueType.Boolean;
            if (p == "float")
                return SlotValueType.Vector1;
            if (p == "float2")
                return SlotValueType.Vector2;
            if (p == "float3")
                return SlotValueType.Vector3;
            if (p == "float4")
                return SlotValueType.Vector4;
            if (p == "Color")
                return SlotValueType.Vector4;
            if (p == "ColorRGBA")
                return SlotValueType.Vector4;
            if (p == "ColorRGB")
                return SlotValueType.Vector3;
            if (p == "Texture2D")
                return SlotValueType.Texture2D;
            if (p == "Texture2DArray")
                return SlotValueType.Texture2DArray;
            if (p == "Texture3D")
                return SlotValueType.Texture3D;
            if (p == "Cubemap")
                return SlotValueType.Cubemap;
            if (p == "UnityTexture2D")
                return SlotValueType.Texture2D;
            if (p == "UnityTexture2DArray")
                return SlotValueType.Texture2DArray;
            if (p == "UnityTexture3D")
                return SlotValueType.Texture3D;
            if (p == "UnityTextureCube")
                return SlotValueType.Cubemap;
            if (p == "Gradient")
                return SlotValueType.Gradient;
            if (p == "SamplerState")
                return SlotValueType.SamplerState;
            if (p == "DynamicDimensionVector")
                return SlotValueType.DynamicVector;
            if (p == "float4x4")
                return SlotValueType.Matrix4;
            if (p == "float3x3")
                return SlotValueType.Matrix3;
            if (p == "float2x2")
                return SlotValueType.Matrix2;
            if (p == "DynamicDimensionMatrix")
                return SlotValueType.DynamicMatrix;
            if (p == "PropertyConnectionState")
                return SlotValueType.PropertyConnectionState;

            throw new ArgumentException("Unsupported type " + p);
        }

    }


    [SGPropertyDrawer(typeof(HLSLNode))]
    public class HLSLNodePropertyDrawer : IPropertyDrawer, IGetNodePropertyDrawerPropertyData
    {
        Action m_setNodesAsDirtyCallback;
        Action m_updateNodeViewsCallback;

        void IGetNodePropertyDrawerPropertyData.GetPropertyData(Action setNodesAsDirtyCallback, Action updateNodeViewsCallback)
        {
            m_setNodesAsDirtyCallback = setNodesAsDirtyCallback;
            m_updateNodeViewsCallback = updateNodeViewsCallback;
        }

        VisualElement CreateGUI(HLSLNode node, InspectableAttribute attribute,
            out VisualElement propertyVisualElement)
        {
            var propertySheet = new PropertySheet(PropertyDrawerUtils.CreateLabel($"{node.name} Node", 0, FontStyle.Bold));

            PropertyDrawerUtils.AddDefaultNodeProperties(propertySheet, node, m_setNodesAsDirtyCallback, m_updateNodeViewsCallback);

            var hlslView = new HlslView(node);
            propertySheet.Add(new HlslView(node));
            propertyVisualElement = null;
            return propertySheet;
        }

        public Action inspectorUpdateDelegate { get; set; }

        public VisualElement DrawProperty(PropertyInfo propertyInfo, object actualObject,
            InspectableAttribute attribute)
        {
            return this.CreateGUI(
                (HLSLNode)actualObject,
                attribute,
                out var propertyVisualElement);
        }

        void IPropertyDrawer.DisposePropertyDrawer() { }
    }



    internal class HlslView : VisualElement
    {
        private EnumField m_Type;
        private ObjectField m_FunctionSource;
        private TextField m_FunctionBody;

        internal HlslView(HLSLNode node)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/HlslFunctionView"));
            Draw(node);
        }

        private void Draw(HLSLNode node)
        {
            var currentControls = this.Children().ToArray();
            for (int i = 0; i < currentControls.Length; i++)
                currentControls[i].RemoveFromHierarchy();

            m_Type = new EnumField(node.sourceType);
            m_Type.RegisterValueChangedCallback(s =>
            {
                if ((HlslSourceType)s.newValue != node.sourceType)
                {
                    node.owner.owner.RegisterCompleteObjectUndo("Change Function Type");
                    node.sourceType = (HlslSourceType)s.newValue;
                    Draw(node);
                    node.ValidateNode();
                    node.Dirty(ModificationScope.Graph);
                    node.UpdateNodeAfterDeserialization();
                }
            });

            string path = AssetDatabase.GUIDToAssetPath(node.functionSource);
            m_FunctionSource = new ObjectField() { value = AssetDatabase.LoadAssetAtPath<ShaderInclude>(path), objectType = typeof(ShaderInclude) };
            m_FunctionSource.RegisterValueChangedCallback(s =>
            {
                long localId;
                string guidString = string.Empty;
                if (s.newValue != null)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier((ShaderInclude)s.newValue, out guidString, out localId);
                }

                if (guidString != node.functionSource)
                {
                    node.owner.owner.RegisterCompleteObjectUndo("Change Function Source");
                    node.functionSource = guidString;
                    node.ValidateNode();
                    node.Dirty(ModificationScope.Graph);
                    node.UpdateNodeAfterDeserialization();
                }
            });
            m_FunctionSource.RegisterCallback<FocusInEvent>(s =>
            {
                node.UpdateNodeAfterDeserialization();
            });

            m_FunctionBody = new TextField { value = node.functionBody, multiline = true };
            m_FunctionBody.RegisterCallback<FocusInEvent>(s =>
            {
                if (m_FunctionBody.value == CustomFunctionNode.defaultFunctionBody)
                    m_FunctionBody.value = "";
            });
            m_FunctionBody.RegisterCallback<FocusOutEvent>(s =>
            {
                if (m_FunctionBody.value == "")
                    m_FunctionBody.value = CustomFunctionNode.defaultFunctionBody;

                if (m_FunctionBody.value != node.functionBody)
                {
                    node.owner.owner.RegisterCompleteObjectUndo("Change Function Body");
                    node.functionBody = m_FunctionBody.value;
                    node.ValidateNode();
                    node.Dirty(ModificationScope.Graph);
                }

                node.UpdateNodeAfterDeserialization();
                Debug.Log("Focus out");
            });
            m_FunctionBody.RegisterValueChangedCallback(s =>
            {
                Debug.Log("Value changed");
                node.UpdateNodeAfterDeserialization();
            });

            VisualElement typeRow = new VisualElement() { name = "Row" };
            {
                typeRow.Add(new Label("Type"));
                typeRow.Add(m_Type);
            }
            Add(typeRow);

            switch (node.sourceType)
            {
                case HlslSourceType.File:
                    VisualElement sourceRow = new VisualElement() { name = "Row" };
                    {
                        sourceRow.Add(new Label("Source"));
                        sourceRow.Add(m_FunctionSource);
                    }
                    Add(sourceRow);
                    break;
                case HlslSourceType.String:
                    VisualElement bodyRow = new VisualElement() { name = "Row" };
                    {
                        bodyRow.Add(new Label("Body"));
                        bodyRow.style.height = 200;
                        bodyRow.Add(m_FunctionBody);
                    }
                    Add(bodyRow);
                    break;
            }
        }
    }
}

#endif