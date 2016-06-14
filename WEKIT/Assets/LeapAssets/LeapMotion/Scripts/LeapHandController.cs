using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using UnityEngine.Networking;

namespace Leap.Unity {
  /**
   * LeapHandController uses a Factory to create and update HandRepresentations based on Frame's received from a Provider  */
  public class LeapHandController : NetworkBehaviour {
    public LeapProvider provider;
    protected HandFactory factory;

        //Felix
        public LeapPlayer Player;
        private List<Hand> _hl;

        protected Dictionary<int, HandRepresentation> graphicsReps = new Dictionary<int, HandRepresentation>();
    protected Dictionary<int, HandRepresentation> physicsReps = new Dictionary<int, HandRepresentation>();

    // Reference distance from thumb base to pinky base in mm.
    protected const float GIZMO_SCALE = 5.0f;

    protected bool graphicsEnabled = true;
    protected bool physicsEnabled = true;

    public bool GraphicsEnabled {
      get {
        return graphicsEnabled;
      }
      set {
        graphicsEnabled = value;
      }
    }

    public bool PhysicsEnabled {
      get {
        return physicsEnabled;
      }
      set {
        physicsEnabled = value;
      }
    }

    /** Draws the Leap Motion gizmo when in the Unity editor. */
    void OnDrawGizmos() {
      Gizmos.matrix = Matrix4x4.Scale(GIZMO_SCALE * Vector3.one);
      Gizmos.DrawIcon(transform.position, "leap_motion.png");
    }

    protected virtual void Start(){
            //provider = requireComponent<LeapProvider>();
            factory = requireComponent<HandFactory>();

            //Felix
            if (provider == null)
            {
                provider = GetComponent<LeapProvider>();
            }
            //Felix
            if (Player == null)
            {
                Player = GetComponent<LeapPlayer>();
            }
    }

    /** Updates the graphics HandRepresentations. */
    protected virtual void Update() {
            /*Frame frame = provider.CurrentFrame;

            if (frame != null && graphicsEnabled) {
              UpdateHandRepresentations(graphicsReps, ModelType.Graphics, frame);
            }*/

            if (!isLocalPlayer)
                return;

        byte[] bytes = Compression.ConvertToBytes<List<Hand>>(provider.CurrentFrame.Hands);
            CmdHandRep(bytes);

            /*
            //Felix
            if (Player == null || !Player.Replaying)
            {
                Frame frame = provider.CurrentFrame;
                if (frame != null && graphicsEnabled)
                {
                    //UpdateHandRepresentations(graphicsReps, ModelType.Graphics, frame);
                    UpdateHandRepresentations(frame.Hands, graphicsReps, ModelType.Graphics);
                }
            }
            else if (Player != null)
            {
                _hl = Player.GetCurrentFrame();
                if ((int)Player.Index != Player.PreviousIndex)
                {
                    UpdateHandRepresentations(_hl, graphicsReps, ModelType.Graphics);
                }
            }
            //Frame frame = Provider.CurrentFrame;
            /*if (frame.Id != prev_graphics_id_ && graphicsEnabled) {
              UpdateHandRepresentations(graphicsReps, ModelType.Graphics);
              prev_graphics_id_ = frame.Id;

            }*/
        }

        [Command]
        void CmdHandRep(byte[] list)
        {
            RpcHandRep(list);
        }

        [ClientRpc]
        void RpcHandRep(byte[] list)
        {
            if (!this.hasAuthority)
            {
                List<Hand> hands = Compression.GetFromBytes<List<Hand>>(list);
                UpdateHandRepresentations(hands, graphicsReps, ModelType.Graphics);
            }
            else
            {
                UpdateHandRepresentations(provider.CurrentFrame.Hands, graphicsReps, ModelType.Graphics);
            }
        }

        /** Updates the physics HandRepresentations. */
        protected virtual void FixedUpdate() {
            /*Frame fixedFrame = provider.CurrentFixedFrame;

            if (fixedFrame != null && physicsEnabled) {
              UpdateHandRepresentations(physicsReps, ModelType.Physics, fixedFrame);
            }*/

            ///////////////////////////////////////////////////////////////

            //All FixedUpdates of a frame happen before Update, so only the last of these calculations is passed
            //into Update for smoothing.
            /*var latestFrame = Provider.CurrentFrame;
            Provider.PerFrameFixedUpdateOffset = latestFrame.Timestamp * NS_TO_S - Time.fixedTime;

            Frame frame = Provider.GetFixedFrame();

            if (frame.Id != prev_physics_id_ && physicsEnabled) {
              UpdateHandRepresentations(physicsReps, ModelType.Physics);
              //UpdateHandModels(hand_physics_, frame.Hands, leftPhysicsModel, rightPhysicsModel); //Originally commented out
              prev_physics_id_ = frame.Id;
            }*/

            //Felix
            if (Player == null || !Player.Replaying)
            {
                Frame fixedFrame = provider.CurrentFixedFrame;

                if (fixedFrame != null && physicsEnabled)
                {
                    UpdateHandRepresentations(physicsReps, ModelType.Physics, fixedFrame);
                }
            }
            else if (Player != null)
            {
                //HandList hl = Player.GetCurrentFrame();
                if ((int) Player.Index != Player.PreviousIndex && _hl != null)
                {
                    UpdateHandRepresentations(_hl, physicsReps, ModelType.Physics);
                }
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
                    rep = factory.MakeHandRepresentation(curHand, modelType);
                    if (rep != null)
                    {
                        all_hand_reps.Add(curHand.Id, rep);
                        //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
                        //rep.handModel.transform.localScale = hand_scale * Vector3.one;
                    }
                }
                if (rep != null)
                {
                    rep.IsMarked = true;
                    //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
                    //rep.handModel.transform.localScale = hand_scale * Vector3.one;
                    rep.UpdateRepresentation(curHand);
                    rep.LastUpdatedTime = (int)provider.CurrentFrame.Timestamp;
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
                        //Debug.Log("LeapHandController Marking False");
                        r.Value.IsMarked = false;
                    }
                    else
                    {
                        //Initialize toBeDeleted with a value to be deleted
                        //Debug.Log("Finishing");
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

        /** 
              * Updates HandRepresentations based in the specified HandRepresentation Dictionary.
              * Active HandRepresentation instances are updated if the hand they represent is still
              * present in the Provider's CurrentFrame; otherwise, the HandRepresentation is removed. If new
              * Leap Hand objects are present in the Leap HandRepresentation Dictionary, new HandRepresentations are 
              * created and added to the dictionary. 
              * @param all_hand_reps = A dictionary of Leap Hand ID's with a paired HandRepresentation
              * @param modelType Filters for a type of hand model, for example, physics or graphics hands.
              * @param frame The Leap Frame containing Leap Hand data for each currently tracked hand
              */
        void UpdateHandRepresentations(Dictionary<int, HandRepresentation> all_hand_reps, ModelType modelType, Frame frame) {
      foreach (Leap.Hand curHand in frame.Hands) {
        HandRepresentation rep;
        if (!all_hand_reps.TryGetValue(curHand.Id, out rep)) {
          rep = factory.MakeHandRepresentation(curHand, modelType);
          if (rep != null) {
            all_hand_reps.Add(curHand.Id, rep);
          }
        }
        if (rep != null) {
          rep.IsMarked = true;
          rep.UpdateRepresentation(curHand);
          rep.LastUpdatedTime = (int)frame.Timestamp;
        }
      }

      /** Mark-and-sweep to finish unused HandRepresentations */
      HandRepresentation toBeDeleted = null;
      foreach (KeyValuePair<int, HandRepresentation> r in all_hand_reps) {
        if (r.Value != null) {
          if (r.Value.IsMarked) {
            r.Value.IsMarked = false;
          } else {
            /** Initialize toBeDeleted with a value to be deleted */
            //Debug.Log("Finishing");
            toBeDeleted = r.Value;
          }
        }
      }
      /**Inform the representation that we will no longer be giving it any hand updates 
       * because the corresponding hand has gone away */
      if (toBeDeleted != null) {
        all_hand_reps.Remove(toBeDeleted.HandID);
        toBeDeleted.Finish();
      }
    }

    private T requireComponent<T>() where T : Component {
      T component = GetComponent<T>();
      if (component == null) {
        string componentName = typeof(T).Name;
        Debug.LogError("LeapHandController could not find a " + componentName + " and has been disabled.  Make sure there is a " + componentName + " on the same gameObject.");
        enabled = false;
      }
      return component;
    }
  }
}
