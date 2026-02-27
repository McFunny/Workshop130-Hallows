using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RandomSelectionReducerWindow : EditorWindow
{
    [SerializeField, Range(0f, 100f)]
    private float keepPercent = 20f;

    [SerializeField]
    private bool useSeed = false;

    [SerializeField]
    private int seed = 12345;

    [SerializeField]
    private bool keepAtLeastOne = true;

    [MenuItem("Tools/Selection/Random Reduce Selection...")]
    private static void Open()
    {
        var window = GetWindow<RandomSelectionReducerWindow>("Random Reduce Selection");
        window.minSize = new Vector2(320, 160);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Random Reduce Selection", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        keepPercent = EditorGUILayout.Slider(new GUIContent("Keep %", "Percent of selected objects to keep selected."),
            keepPercent, 0f, 100f);

        keepAtLeastOne = EditorGUILayout.ToggleLeft(
            new GUIContent("Keep at least 1 (if selection not empty)", "Prevents ending with 0 objects when % is very small."),
            keepAtLeastOne);

        useSeed = EditorGUILayout.ToggleLeft(new GUIContent("Use seed (repeatable)", "Same seed + same selection order => same result."), useSeed);
        using (new EditorGUI.DisabledScope(!useSeed))
        {
            seed = EditorGUILayout.IntField("Seed", seed);
        }

        EditorGUILayout.Space(10);

        var selectionCount = Selection.objects?.Length ?? 0;
        EditorGUILayout.LabelField($"Currently selected: {selectionCount}");

        using (new EditorGUI.DisabledScope(selectionCount == 0))
        {
            if (GUILayout.Button("Reduce Selection"))
            {
                ReduceSelection();
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox("This keeps a random subset of the current selection and deselects the rest.", MessageType.Info);
    }

    private void ReduceSelection()
    {
        UnityEngine.Object[] original = Selection.objects;
        if (original == null || original.Length == 0)
            return;

        int total = original.Length;

        // Calculate how many to keep.
        // Example: 10 * 20% = 2.
        int keepCount = Mathf.RoundToInt(total * (keepPercent / 100f));

        if (keepAtLeastOne && total > 0)
            keepCount = Mathf.Max(1, keepCount);

        keepCount = Mathf.Clamp(keepCount, 0, total);

        // Nothing to keep => clear selection.
        if (keepCount == 0)
        {
            Selection.objects = Array.Empty<UnityEngine.Object>();
            return;
        }

        // Shuffle indices, pick first keepCount.
        var indices = new List<int>(total);
        for (int i = 0; i < total; i++)
            indices.Add(i);

        var rng = useSeed ? new System.Random(seed) : new System.Random(Guid.NewGuid().GetHashCode());

        // Fisher-Yates shuffle.
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        var kept = new UnityEngine.Object[keepCount];
        for (int k = 0; k < keepCount; k++)
            kept[k] = original[indices[k]];

        Selection.objects = kept;
    }
}