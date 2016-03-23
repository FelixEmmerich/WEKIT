﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace Leap {

  [ExecuteAfter(typeof(LeapProvider))]
  public class LeapHandController : MonoBehaviour {
    /** The scale factors for hand movement. Set greater than 1 to give the hands a greater range of motion. */
    public Vector3 handMovementScale = Vector3.one;

        //Felix
    //public LeapProvider Provider { get; set; }
      public LeapProvider Provider;

    public HandFactory Factory { get; set; }

        //Felix
        public LeapPlayer Player;
        private HandList _hl; 

    public Dictionary<int, HandRepresentation> graphicsReps = new Dictionary<int, HandRepresentation>();
    public Dictionary<int, HandRepresentation> physicsReps = new Dictionary<int, HandRepresentation>();

    // Reference distance from thumb base to pinky base in mm.
    protected const float GIZMO_SCALE = 5.0f;
    /** Conversion factor for millimeters to meters. */
    protected const float MM_TO_M = 1e-3f;
    /** Conversion factor for nanoseconds to seconds. */
    protected const float NS_TO_S = 1e-6f;
    /** Conversion factor for seconds to nanoseconds. */
    protected const float S_TO_NS = 1e6f;
    /** How much smoothing to use when calculating the FixedUpdate offset. */
    protected const float FIXED_UPDATE_OFFSET_SMOOTHING_DELAY = 0.1f;

    protected bool graphicsEnabled = true;
    protected bool physicsEnabled = true;

    public bool GraphicsEnabled {
      get {
        return graphicsEnabled;
      }
      set {
        graphicsEnabled = value;
        if (!graphicsEnabled) {
          //DestroyGraphicsHands();
        }
      }
    }

    public bool PhysicsEnabled {
      get {
        return physicsEnabled;
      }
      set {
        physicsEnabled = value;
        if (!physicsEnabled) {
          //DestroyPhysicsHands();
        }
      }
    }
    private long prev_graphics_id_ = 0;
    private long prev_physics_id_ = 0;

    /** Draws the Leap Motion gizmo when in the Unity editor. */
    /*
    void OnDrawGizmos() {
      // Draws the little Leap Motion Controller in the Editor view.
      Gizmos.matrix = Matrix4x4.Scale(GIZMO_SCALE * Vector3.one);
      Gizmos.DrawIcon(transform.position, "leap_motion.png");
    }
    */

    // Use this for initialization
    void Start()
    {
            //Felix
        if (Provider == null)
        {
                Provider = GetComponent<LeapProvider>();
            }
      //Provider = GetComponent<LeapProvider>();
      Factory = GetComponent<HandFactory>();
            //Felix
        if (Player == null)
        {
            Player = GetComponent<LeapPlayer>();
        }
    }
    /**
    * Turns off collisions between the specified GameObject and all hands.
    * Subject to the limitations of Unity Physics.IgnoreCollisions(). 
    * See http://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html.
    */
    public void IgnoreCollisionsWithHands(GameObject to_ignore, bool ignore = true) {
      foreach (HandRepresentation rep in physicsReps.Values) {
        //Todo move this to HandModel
        //Leap.Utils.IgnoreCollisions(rep.handModel.gameObject, to_ignore, ignore);
      }
    }
    void Update()
        {
      //Felix
        if (Player==null||!Player.Replaying)
        {
            Frame frame = Provider.CurrentFrame;
            if (frame.Id != prev_graphics_id_ && graphicsEnabled)
            {
                UpdateHandRepresentations(graphicsReps, ModelType.Graphics);
                prev_graphics_id_ = frame.Id;
            }
        }
        else if (Player!=null)
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
        //Felix
        void UpdateHandRepresentations(HandList list, Dictionary<int, HandRepresentation> all_hand_reps, ModelType modelType)
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
                        //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
                        //rep.handModel.transform.localScale = hand_scale * Vector3.one;
                    }
                }
                if (rep != null)
                {
                    rep.IsMarked = true;
                    //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
                    //rep.handModel.transform.localScale = hand_scale * Vector3.one;
                    rep.UpdateRepresentation(curHand, modelType);
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

        void UpdateHandRepresentations(Dictionary<int, HandRepresentation> all_hand_reps, ModelType modelType) {
      foreach (Leap.Hand curHand in Provider.CurrentFrame.Hands) {
        HandRepresentation rep;
        if (!all_hand_reps.TryGetValue(curHand.Id, out rep)) {
          rep = Factory.MakeHandRepresentation(curHand, modelType);
          if (rep != null) {
            all_hand_reps.Add(curHand.Id, rep);
            //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
            //rep.handModel.transform.localScale = hand_scale * Vector3.one;
          }
        }
        if (rep != null) {
          rep.IsMarked = true;
          //float hand_scale = curHand.PalmWidth / rep.handModel.handModelPalmWidth;
          //rep.handModel.transform.localScale = hand_scale * Vector3.one;
          rep.UpdateRepresentation(curHand, modelType);
          rep.LastUpdatedTime = (int)Provider.CurrentFrame.Timestamp;
        }
      }

      //Mark-and-sweep or set difference implementation
      HandRepresentation toBeDeleted = null;
      foreach (KeyValuePair<int, HandRepresentation> r in all_hand_reps) {
        if (r.Value != null) {
          if (r.Value.IsMarked) {
            //Debug.Log("LeapHandController Marking False");
            r.Value.IsMarked = false;
          }
          else {
            //Initialize toBeDeleted with a value to be deleted
            //Debug.Log("Finishing");
            toBeDeleted = r.Value;
          }
        }
      }
      //Inform the representation that we will no longer be giving it any hand updates
      //because the corresponding hand has gone away
      if (toBeDeleted != null) {
        all_hand_reps.Remove(toBeDeleted.HandID);
        toBeDeleted.Finish();
      }
    }
    /** Updates the physics objects */
    protected virtual void FixedUpdate() {

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
            if (Player==null||!Player.Replaying)
            {
                Frame frame = Provider.CurrentFrame;
                Provider.PerFrameFixedUpdateOffset = frame.Timestamp * NS_TO_S - Time.fixedTime;
                if (frame.Id != prev_physics_id_ && physicsEnabled)
                {
                    UpdateHandRepresentations(physicsReps, ModelType.Physics);
                    prev_physics_id_ = frame.Id;
                }
            }
            else if (Player!=null)
            {
                //HandList hl = Player.GetCurrentFrame();
                if ((int)Player.Index != Player.PreviousIndex && _hl!=null)
                {
                    UpdateHandRepresentations(_hl, physicsReps, ModelType.Physics);
                }
            }

        }
  }
}
