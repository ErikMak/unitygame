using UnityEngine;
using UnityEngine.UIElements;

internal sealed class HanseaticDocumentHost : MonoBehaviour
{
    private UIDocument document;
    private VisualElement content;

    public void Bind(UIDocument uiDocument, VisualElement view)
    {
        document = uiDocument;
        content = view;
        AttachView();
    }

    private void LateUpdate() => AttachView();

    // Unity can recreate the document root when active panels change during a scene load.
    private void AttachView()
    {
        VisualElement root = document != null ? document.rootVisualElement : null;
        if (root == null || content == null || content.parent == root)
            return;

        root.pickingMode = PickingMode.Ignore;
        root.Add(content);
    }
}
