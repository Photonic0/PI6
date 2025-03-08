using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Common.Systems
{
    public class Checkpoint : MonoBehaviour
    {
        //index is assigned by multi scene singleton populator
        public int index;
        [SerializeField] GameObject[] objsToDespawn;
        VerletSimulator flagVerletSimulation;
        [SerializeField] LineRenderer lineRenderer;
        const float OscillationSpeed = 0.7f;
        const float OscillationArc = 0.7f;
        Mesh flagMesh;
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] MeshRenderer meshRenderer;
        Color meshColor;
        [SerializeField] Texture flagTexture;
        [SerializeField] SpriteRenderer[] spritesToColor;
        //add like uhh sparkles or something to indicate that the checkpoint is set
        const int MeshWidth = 25;
        const int MeshHeight = 15;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(Tags.Player))
            {
                if(LevelInfo.latestCheckpointIndex != index)
                {
                    GetComponent<AudioSource>().volume = 0.8f * Settings.sfxVolume;
                    GetComponent<AudioSource>().Play();
                    GameManager.PlayerLife.HealMax();
                }
                LevelInfo.latestCheckpointIndex = index;
            }
        }
        private void Start()
        {
            for (int i = 0; i < spritesToColor.Length; i++)
            {
                spritesToColor[i].color = LevelInfo.LevelColor;

            }
            InitializeFlag();
        }

        private void InitializeFlag()
        {
            //first arg = mass
            //second arg = iterations
            flagVerletSimulation = new(1, 5);
            float unitPerDot = 0.05f;

            Vector2 origin = transform.position;
            List<Dot> dots = new(MeshWidth * MeshHeight);
            //create a square of dots, starting at bottom left, in row order.
            for (int j = 0; j < MeshHeight; j++)
            {
                for (int i = 0; i < MeshWidth; i++)
                {
                    Vector2 initialPosOffset = new(i * unitPerDot, j * unitPerDot);
                    bool dotCantMove = i == 0;
                    dots.Add(new Dot(origin + initialPosOffset, dotCantMove));
                }
            }
            //connect each dot to the one below it it
            //VERTICAL CONNECTIONS
            for (int i = MeshWidth; i < dots.Count; i++)
            {
                Dot.Connect(dots[i], dots[i - MeshWidth]);
            }
            //connect each dot to the dot to the previous dot
            //only if i % meshWidth != 0 so that it doesn't connect the first dot of a row with the last of the previous row
            //HORIZONTAL CONNECTIONS
            for (int i = 1; i < dots.Count; i++)
            {
                if (i % MeshWidth != 0)
                {
                    Dot.Connect(dots[i], dots[i - 1]);
                }
            }
            flagVerletSimulation.dots = dots;
            Vector3[] positions = GetLineRendererPositions(dots);
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
            lineRenderer.widthMultiplier = 3f / 50f;
            lineRenderer.startColor = FlipnoteColors.DarkGreen;
            lineRenderer.endColor = FlipnoteColors.DarkGreen;
            SetMeshData(dots);
            meshColor = FlipnoteColors.DarkGreen;
            meshColor.r = Mathf.GammaToLinearSpace(meshColor.r);
            meshColor.g = Mathf.GammaToLinearSpace(meshColor.g);
            meshColor.b = Mathf.GammaToLinearSpace(meshColor.b);
            meshColor.a = Mathf.GammaToLinearSpace(meshColor.a);
            meshRenderer.material.SetTexture("_MainTex", flagTexture);
            meshFilter.mesh = flagMesh;
        }

        private void SetMeshData(List<Dot> dots)
        {
            if (flagMesh == null)
            {
                flagMesh = new Mesh();
            }
            int length = dots.Count;
            Vector3[] positions = new Vector3[length];
            Vector3[] normals = new Vector3[length];
            List<int> indices = new(length * 3 - 6);
            Color[] colors = new Color[length];
            Vector2 meshOffset = transform.position;
            for (int i = 0; i < length; i++)
            {
                Vector2 posToSet = (Vector3)dots[i].position;
                posToSet -= meshOffset;
                positions[i] = posToSet;
                normals[i] = Vector3.forward;
                colors[i] = meshColor;
            }
            for (int i = 0; i < length - MeshWidth - 1; i++)
            {
                indices.Add(i);
                indices.Add(i + MeshWidth);
                indices.Add(i + 1);

                indices.Add(i + 1);
                indices.Add(i + MeshWidth);
                indices.Add(i + MeshWidth + 1);
            }
            flagMesh.Clear();
            flagMesh.SetVertices(positions);
            flagMesh.SetNormals(normals);
            flagMesh.SetColors(colors);
            flagMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
        private static Vector3[] GetLineRendererPositions(List<Dot> dots)
        {
            int index = 0;
            Vector3[] positions = new Vector3[MeshHeight - 2 + MeshWidth * 2];
            //lower edge
            for (int i = 0; i < MeshWidth; i++)
            {
                positions[index] = dots[i].position;
                index++;
            }
            //front edge, except top right and bottom right vertices
            if (MeshHeight > 2)
            {
                for (int i = 2 * MeshWidth - 1; i < dots.Count - 1; i += MeshWidth)
                {
                    positions[index] = dots[i].position;
                    index++;
                }
            }
            //upper edge
            for (int i = dots.Count - 1; i >= MeshWidth * MeshHeight - MeshWidth; i--)
            {
                positions[index] = dots[i].position;
                index++;
            }

            return positions;
        }

        //private void FixedUpdate()
        //{
        //    //UpdateFlag();
        //}

        private void UpdateFlag()
        {
            if ((GameManager.PlayerPosition - transform.position).magnitude < 12)//distance culling of physics simulation
            {
                Vector2 force = GetVerletForce();
                flagVerletSimulation.AddForce(force);
                flagVerletSimulation.Simulate(Time.fixedDeltaTime);
                List<Dot> dots = flagVerletSimulation.dots;
                Vector3[] positions = GetLineRendererPositions(dots);
                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);
                SetMeshData(flagVerletSimulation.dots);
            }
        }

        private Vector2 GetVerletForce()
        {
            float coord = Time.time * OscillationSpeed;
            return (Mathf.Lerp(-1, 1, Mathf.PerlinNoise(coord, coord * Helper.Phi)) * OscillationArc + Mathf.PI / 2f).PolarVector_Old();
        }

        //make int and label each checkpoint with an index
        //keep int[] data through the scene reload
        //that way you kinda keep the reference, because the checkpoint obj will be replaced by a new one 
        public void RespawnAt()
        {
            for (int i = 0; i < objsToDespawn.Length; i++)
            {
                objsToDespawn[i].SetActive(false);
            }
            PlayerControl player = GameManager.PlayerControl;
            Vector3 spawnPoint = transform.position;
            player.transform.position = spawnPoint;
            GameManager.PlayerLife.HealMax();
            PlayerWeaponManager.RechargeAll();
            Helper.ResetCamera(spawnPoint);
        }

        //DEBUG STUFF BELOW
#if UNITY_EDITOR
        [SerializeField] new BoxCollider2D collider;
        private void OnDrawGizmos()
        {
            if (collider != null)
            {
                Gizmos.color = Color.green;
                Gizmos2.DrawLinesInBoxCollider(collider, 2, true);
            }
            Gizmos.color = new Color(1, 0, 1, 0.5f);
            Gizmos.DrawCube(transform.position, new Vector3(1.4f, 2, 0.005f));
            //if (flagVerletSimulation != null && flagVerletSimulation.dots != null)
            //{
            //    for (int i = 0; i < flagVerletSimulation.dots.Count; i++)
            //    {
            //        List<DotConnection> connections = flagVerletSimulation.dots[i].connections;
            //        if (connections.Count > 0)
            //        {
            //            for (int j = 0; j < connections.Count; j++)
            //            {
            //                Gizmos.DrawLine(connections[j].dotA.position, connections[j].dotB.position);
            //            }
            //        }
            //    }
            //    Vector3[] positions = GetLineRendererPositions(flagVerletSimulation.dots);
            //    for (int i = 0; i < positions.Length; i++)
            //    {
            //        Gizmos.DrawSphere(positions[i], .025f);
            //    }
            //}
            //Gizmos2.DrawArrow(transform.position, GetVerletForce());
            if (flagMesh != null)
            {
                Gizmos.DrawMesh(flagMesh, transform.position);
            }
        }
#endif
    }
}
