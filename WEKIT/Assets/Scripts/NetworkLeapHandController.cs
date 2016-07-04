using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Networking;

namespace Leap.Unity
{
    /**
     * LeapHandController uses a Factory to create and update HandRepresentations based on Frame's received from a Provider  */
    public class NetworkLeapHandController : NetworkBehaviour
    {
        public LeapProvider Provider;
        protected HandFactory Factory;

        //Channel should be Reliable Fragmented, Network Manager does not save this
        public const int Chunnel=1;

        private int _counter = 0;

        private byte[] _bytes;
        private byte[] _lastFrame;
        

        //Felix
        public bool Server;

        protected Dictionary<int, HandRepresentation> graphicsReps = new Dictionary<int, HandRepresentation>();
        protected Dictionary<int, HandRepresentation> physicsReps = new Dictionary<int, HandRepresentation>();

        // Reference distance from thumb base to pinky base in mm.
        protected const float GIZMO_SCALE = 5.0f;

        protected bool graphicsEnabled = true;
        protected bool physicsEnabled = true;

        public bool GraphicsEnabled
        {
            get
            {
                return graphicsEnabled;
            }
            set
            {
                graphicsEnabled = value;
            }
        }

        public bool PhysicsEnabled
        {
            get
            {
                return physicsEnabled;
            }
            set
            {
                physicsEnabled = value;
            }
        }

        /** Draws the Leap Motion gizmo when in the Unity editor. */
        void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.Scale(GIZMO_SCALE * Vector3.one);
            Gizmos.DrawIcon(transform.position, "leap_motion.png");
        }

        protected virtual void Start()
        {
            Factory = requireComponent<HandFactory>();

            //Felix
            if (Provider == null)
            {
                Provider = GetComponent<LeapProvider>();
            }
        }

        /** Updates the graphics HandRepresentations. */
        protected virtual void Update()
        {

            if (((isServer || isClient) && (Server != isServer))||Provider.CurrentFrame.Hands.Count==0)
                return;

            if (_counter == 0)
            {
                _lastFrame = Compression.ConvertToBytes(Provider.CurrentFrame.Hands);
            }

            if (!isServer)
            {
                if (_counter == 0)
                {
                    byte[] rv = new byte[_lastFrame.Length / 2];
                    System.Buffer.BlockCopy(_lastFrame, 0, rv, 0, _lastFrame.Length / 2 * sizeof(byte));
                    CmdHandRep(rv);
                }
                else
                {
                    byte[] rv = new byte[_lastFrame.Length - (_lastFrame.Length / 2)];
                    System.Buffer.BlockCopy(_lastFrame, _lastFrame.Length / 2 * sizeof(byte), rv, 0, (_lastFrame.Length - (_lastFrame.Length / 2)) * sizeof(byte));
                    CmdHandRep2(rv);
                }
            }
            else
            {
                if (_counter == 0)
                {
                    byte[] rv = new byte[_lastFrame.Length / 2];
                    System.Buffer.BlockCopy(_lastFrame, 0, rv, 0, _lastFrame.Length / 2*sizeof(byte));
                    RpcHandRep(rv);
                }
                else
                {
                    byte[] rv = new byte[_lastFrame.Length - (_lastFrame.Length / 2)];
                    System.Buffer.BlockCopy(_lastFrame, _lastFrame.Length / 2 * sizeof(byte), rv, 0, (_lastFrame.Length - (_lastFrame.Length / 2))*sizeof(byte));
                    RpcHandRep2(rv);
                }
            }
        }

        [Command(channel=Chunnel)]
        void CmdHandRep(byte[] list)
        {
            _counter = 1;
            _bytes = list;
            //List<Hand> hands = Compression.GetFromBytes<List<Hand>>(list);
            //UpdateHandRepresentations(hands, graphicsReps, ModelType.Graphics);
        }

        [Command(channel = Chunnel)]
        void CmdHandRep2(byte[] list)
        {
            _counter = 0;

            byte[] rv = new byte[_bytes.Length + list.Length];
            System.Buffer.BlockCopy(_bytes, 0, rv, 0, _bytes.Length);
            System.Buffer.BlockCopy(list, 0, rv, _bytes.Length, list.Length);

            List<Hand> hands = Compression.GetFromBytes<List<Hand>>(rv);
            UpdateHandRepresentations(hands, graphicsReps, ModelType.Graphics);
        }

        [ClientRpc(channel=Chunnel)]
        void RpcHandRep(byte[] list)
        {
            _counter = 1;
            _bytes = list;
            if (!this.hasAuthority)
            {
                //List<Hand> hands = Compression.GetFromBytes<List<Hand>>(list);
                //UpdateHandRepresentations(hands, graphicsReps, ModelType.Graphics);
            }
            else
            {
                UpdateHandRepresentations(Provider.CurrentFrame.Hands, graphicsReps, ModelType.Graphics);
            }
        }

        [ClientRpc(channel = Chunnel)]
        void RpcHandRep2(byte[] list)
        {
            _counter = 0;
            if (!this.hasAuthority)
            {
                byte[] rv = new byte[_bytes.Length + list.Length];
                System.Buffer.BlockCopy(_bytes, 0, rv, 0, _bytes.Length);
                System.Buffer.BlockCopy(list, 0, rv, _bytes.Length, list.Length);

                List<Hand> hands = Compression.GetFromBytes<List<Hand>>(rv);
                UpdateHandRepresentations(hands, graphicsReps, ModelType.Graphics);
            }
            else
            {
                UpdateHandRepresentations(Provider.CurrentFrame.Hands, graphicsReps, ModelType.Graphics);
            }
        }

        //Felix
        void UpdateHandRepresentations(List<Hand> list, Dictionary<int, HandRepresentation> all_hand_reps, ModelType modelType)
        {
            foreach (Leap.Hand curHand in list)
            {
                HandRepresentation rep;
                if (!all_hand_reps.TryGetValue(curHand.Id, out rep))
                {
                    rep = Factory.MakeHandRepresentation(curHand, modelType);
                    if (rep != null)
                    {
                        all_hand_reps.Add(curHand.Id, rep);
                    }
                }
                if (rep != null)
                {
                    rep.IsMarked = true;
                    rep.UpdateRepresentation(curHand);
                    rep.LastUpdatedTime = (int)Provider.CurrentFrame.Timestamp;
                }
            }

            //Mark-and-sweep or set difference implementation
            HandRepresentation toBeDeleted = null;
            foreach (KeyValuePair<int, HandRepresentation> r in all_hand_reps)
            {
                if (r.Value != null)
                {
                    if (r.Value.IsMarked)
                    {
                        r.Value.IsMarked = false;
                    }
                    else
                    {
                        toBeDeleted = r.Value;
                    }
                }
            }
            //Inform the representation that we will no longer be giving it any hand updates
            //because the corresponding hand has gone away
            if (toBeDeleted != null)
            {
                all_hand_reps.Remove(toBeDeleted.HandID);
                toBeDeleted.Finish();
            }
        }

        private T requireComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                string componentName = typeof(T).Name;
                Debug.LogError("LeapHandController could not find a " + componentName + " and has been disabled.  Make sure there is a " + componentName + " on the same gameObject.");
                enabled = false;
            }
            return component;
        }
    }
}