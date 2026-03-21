using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    [Serializable]
    private struct TeamMaterialBinding
    {
        public int TeamId;
        public Material Material;
    }

    [SerializeField] private int _teamId = 1;
    [SerializeField] private Renderer[] _teamRenderers;
    [SerializeField] private List<TeamMaterialBinding> _teamMaterials = new();

    public int TeamId => _teamId;

    private void Awake()
    {
        ApplyTeamMaterial();
    }

    private void OnValidate()
    {
        ApplyTeamMaterial();
    }

    public bool IsHostileTo(TeamMember other)
    {
        return other != null && other != this && _teamId != other._teamId;
    }

    public void SetTeamId(int teamId)
    {
        _teamId = teamId;
        ApplyTeamMaterial();
    }

    private void ApplyTeamMaterial()
    {
        Material material = GetMaterialForCurrentTeam();
        Renderer[] targetRenderers = _teamRenderers != null && _teamRenderers.Length > 0
            ? _teamRenderers
            : new[] { GetComponent<Renderer>() };

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.sharedMaterial = material;
        }
    }

    private Material GetMaterialForCurrentTeam()
    {
        foreach (TeamMaterialBinding teamMaterial in _teamMaterials)
        {
            if (teamMaterial.TeamId == _teamId && teamMaterial.Material != null)
            {
                return teamMaterial.Material;
            }
        }

        return null;
    }
}
