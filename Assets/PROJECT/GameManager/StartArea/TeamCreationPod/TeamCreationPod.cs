﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

enum ETeamCreationPod
{
    OnePlayer,
    TwoPlayers,
    ThreePlayers
}

public class TeamCreationPod : MonoBehaviour
{
    [SerializeField]
    ETeamCreationPod teamSize;

    //public static int teamSizeInt = 0;

    [SerializeField]
    Color teamColor;

    [SerializeField]
    MeshRenderer capacityIndicator;

    [SerializeField]
    Material availableCapacityMaterial;

    [SerializeField]
    Material fullCapacityMaterial;

    [SerializeField]
    MeshRenderer floor;

    [SerializeField]
    MeshRenderer readyIndicator;

    [SerializeField]
    InteractButton readyButton;

    TextMeshPro readyText;

    [SerializeField]
    TextMeshPro teamSizeText;

    [SerializeField]
    Color AvatarTorsoDefault;

    Material teamNotReadyMaterial;
    Material teamReadyMaterial;


    bool teamFilledUp = false;

    public bool TeamFilledUp { get => teamFilledUp; set => teamFilledUp = value; }

    bool teamEmpty = true;

    bool readyToPlay;

    public bool ReadyToPlay { get => readyToPlay; set => readyToPlay = value; }

    public static List<TeamCreationPod> instances = new List<TeamCreationPod>();

    public static Dictionary<Color, int> ColorToTeamSize = new Dictionary<Color, int>();

    


    List<int> teamMembers = new List<int>();
    public List<int> TeamMembers { get => teamMembers; }

    private void Awake()
    {
        if (instances.Count > 0) instances.Clear();
        if (ColorToTeamSize.Count > 0) ColorToTeamSize.Clear();

        //if (teamSizeInt != 0) teamSizeInt = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);

        Debug.Log("TCP: Added this to instances. Count is now: " + instances.Count);

        int teamSizeInt = (int)teamSize + 1;

        ColorToTeamSize.Add(teamColor, teamSizeInt);

        //teamSizeInt = (int)teamSize + 1;

        for (int i = 0; i < teamSizeInt; i++)
            teamMembers.Add(-1);

        teamSizeText.text = teamSizeInt.ToString() + "\nPlayer" + (teamSizeInt > 1 ? "s" : "");

        floor.material.SetColor("_BaseColor",teamColor);

        readyButton.OnExecute += SetReady;


        teamNotReadyMaterial = availableCapacityMaterial;
        teamReadyMaterial = fullCapacityMaterial;

        readyText = readyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    //Add player to team
                    if (teamMembers[i] == -1)
                    {
                        teamMembers[i] = rv.ownerIDSelf;
                        teamEmpty = false;

                        PlayerSync ps = other.GetComponent<PlayerSync>();
                        if (ps) ps.PlayerTorso.material.SetColor("_BaseColor", teamColor);

                        teamFilledUp = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] == -1) teamFilledUp = false;

                        if (teamFilledUp) capacityIndicator.material = fullCapacityMaterial;
                        
                        break;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if (teamMembers[i] == rv.ownerIDSelf)
                    {
                        teamMembers[i] = -1;

                        PlayerSync ps = other.GetComponent<PlayerSync>();
                        if (ps) ps.PlayerTorso.material.SetColor("_BaseColor", AvatarTorsoDefault);

                        capacityIndicator.material = availableCapacityMaterial;

                        teamFilledUp = readyToPlay = false;
                        readyIndicator.material = teamNotReadyMaterial;

                        teamEmpty = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] != -1) teamEmpty = false;

                            break;
                    }
                }
            }
        }
    }

    void SetReady()
    {
        Debug.Log("TCP: SetReady() called");

        readyToPlay = teamFilledUp;

        if (readyToPlay) readyIndicator.material = teamReadyMaterial;
        else
        {
            Debug.Log("TCP: Team in " + name + " is not ready to play. Aborting");
            return;
        }

        int readyTeams = 0;
        int emptyPods = 0;

        foreach (TeamCreationPod pod in instances)
        {
            if (pod.readyToPlay) readyTeams++;
            else if (pod.teamEmpty) emptyPods++;
        }

        bool allPodsReadyTeamsOrEmpty = (readyTeams + emptyPods) == instances.Count;

        bool allPlayersAccountedFor = GalacticGamesManager.Instance.AllPlayersAccountedFor();

        if (allPodsReadyTeamsOrEmpty && allPlayersAccountedFor)
        {
            foreach (TeamCreationPod pod in instances)
                if (pod.readyToPlay) pod.readyText.text = "GO!";


            Debug.Log("TCP: All pods have ready teams or is empty and all players accounted for. Calling GameManager::StartGame()");
            GalacticGamesManager.Instance.StartGame();
        }
        else
        {
            if (!allPodsReadyTeamsOrEmpty) Debug.Log("GGM: Cannot start game because all pods do not have ready teams or are empty");
            if (!allPlayersAccountedFor) Debug.Log("GGM: Cannot start game because not all players are accounted for");
        }
    }

    public void OnTeamMemberLeftRoom(int teamMemberIndex)
    {
        teamMembers[teamMemberIndex] = -1;
        teamFilledUp = readyToPlay = false;

        readyIndicator.material = teamNotReadyMaterial;
    }
}
