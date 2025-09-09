using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterAction : MonoBehaviour
{
    public List<ActionBehavior> actions;
    public ActionBehavior activeAction;

    void Start()
    {
        actions = GetComponentsInChildren<ActionBehavior>().ToList();

        Debug.Assert(actions.Count > 0);

        actions.Sort((a, b) => a.id.CompareTo(b.id));

        for (int i = 1; i < actions.Count; i++)
        {
            Debug.Assert(actions[i].id != actions[i - 1].id,  $"Duplicate ID!! {actions[i].id}");
        }

        activeAction = null;
    }

    private void Update()
    {
        if (activeAction != null && !activeAction.isRunning) 
        {
            activeAction = null;
        }
    }

    public bool TryRun(int id)
    {
        Debug.Log("run action");

        if (id >= actions.Count) 
        {
            Debug.LogWarning("Tried to trigger a non-existing action!");
            return false;
        }

        if (activeAction != null || actions[id].isOnCoolDown)
        {
            return false;
        }

        activeAction = actions[id];
        activeAction.enabled = true;
        return true;
    }
}
