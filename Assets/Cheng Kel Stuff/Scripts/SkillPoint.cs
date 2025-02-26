using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPoint : MonoBehaviour
{
    private SkillTree _skillTree;

    private void Start()
    {
        _skillTree = FindObjectOfType<SkillTree>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _skillTree.IncreaseSkillPoint();
            Destroy(gameObject);
        }
    }
}