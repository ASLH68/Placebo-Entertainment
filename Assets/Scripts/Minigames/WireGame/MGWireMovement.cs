/*****************************************************************************
// File Name :         MGWireMovement.cs
// Author :            Andrea Swihart-DeCoster
// Creation Date :     05/18/23
//
// Brief Description : Controls the wire creation and movement. Huge credit to
                       https://www.youtube.com/watch?v=8rI1D1YQmhM
*****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Rendering.HableCurve;

public class MGWireMovement : MonoBehaviour
{
    private Transform[] _segments;
    [SerializeField] Transform _segmentParent;
    [SerializeField] Transform _startTrans, _endTrans;
    [SerializeField] int _segmentCount = 10;
    [SerializeField] float _totalLength = 10f;
    [SerializeField] float _radius = 0.5f;

    [SerializeField] float _totalWeight = 10f;


    [SerializeField] float _drag = 1f;
    [SerializeField] float _angularDrag = 1f;

    [SerializeField] bool _usePhysics = false;

    private void Start()
    {
        //_segments = new Transform[_segmentCount];
        //GenerateSegments();
    }

    /// <summary>
    /// Spawns wire segments
    /// </summary>
    //private void GenerateSegments()
    //{
    //    JoinSegment(_startTrans, null, true);
    //    Transform prevTrans = _startTrans;

    //    Vector3 direction = (_endTrans.position - _startTrans.position);

    //    for (int i = 0; i < _segmentCount; i++)
    //    {
    //        GameObject segment = new GameObject($"segment_{i}");
    //        segment.transform.SetParent(_segmentParent);
    //        _segments[i] = segment.transform;

    //        Vector3 pos = prevTrans.position + (direction / _segmentCount);
    //        segment.transform.position = pos;

    //        JoinSegment(segment.transform, prevTrans);

    //        prevTrans = segment.transform;

    //        Debug.Log(segment.transform.rotation);

    //        GenerateSphereObj(_segments[i], (i == _segmentCount - 1));

    //        Debug.Log(segment.transform.rotation);
    //    }

    //    JoinSegment(_endTrans, prevTrans, true, true);
    //}

    /// <summary>
    /// Connects a segment to the previous segment
    /// </summary>
    /// <param name="currentTrans">current being added</param>
    /// <param name="connectedTrans">segment that is being connected to</param>
    /// <param name="isKinematic">rb kinematic</param>
    /// <param name="isCloseConnected">is last connection</param>
    //private void JoinSegment(Transform currentTrans, Transform connectedTrans,
    //    bool isKinematic = false, bool isCloseConnected = false)
    //{
    //    Rigidbody rb = currentTrans.AddComponent<Rigidbody>();

    //    rb.isKinematic = isKinematic;
    //    rb.mass = _totalWeight / _segmentCount;
    //    rb.drag = _drag;
    //    rb.angularDrag = _angularDrag;

    //    if (_usePhysics)
    //    {
    //        //CapsuleCollider capsuleCollider = currentTrans.AddComponent<CapsuleCollider>();
    //        SphereCollider sphereCollider = currentTrans.AddComponent<SphereCollider>();
    //        sphereCollider.radius = _radius;
    //        //capsuleCollider.radius = _radius;
    //    }

    //    if (connectedTrans != null)
    //    {
    //        ConfigurableJoint joint = currentTrans.AddComponent<ConfigurableJoint>();

    //        joint.connectedBody = connectedTrans.GetComponent<Rigidbody>();

    //        joint.autoConfigureConnectedAnchor = false;
    //        if (isCloseConnected)
    //        {
    //            // Applies to end
    //            joint.connectedAnchor = Vector3.forward * 0.1f; // I think this is where the problems lie, how???
    //            Debug.DrawRay(joint.transform.position, new Vector3(1, 1, 1), color: Color.red, 20f);

    //            joint.GetComponent<Rigidbody>().freezeRotation = true;
    //        }
    //        else
    //        {
    //            // Applies to start and other segments
    //            joint.connectedAnchor =
    //                Vector3.forward * (_totalLength / _segmentCount);
    //            Debug.DrawRay(joint.transform.position, new Vector3(1, 1, 1), color: Color.red, 20f);
    //        }

    //        joint.xMotion = ConfigurableJointMotion.Locked;
    //        joint.yMotion = ConfigurableJointMotion.Locked;
    //        joint.zMotion = ConfigurableJointMotion.Locked;

    //        // Joints can rotate sideways and up + down and not rotate between
    //        // the segments
    //        joint.angularXMotion = ConfigurableJointMotion.Free;
    //        joint.angularYMotion = ConfigurableJointMotion.Free;
    //        joint.angularZMotion = ConfigurableJointMotion.Limited;

    //        SoftJointLimit softJointLimit = new SoftJointLimit();
    //        softJointLimit.limit = 0;
    //        joint.angularZLimit = softJointLimit;

    //        JointDrive jointDrive = new JointDrive();
    //        jointDrive.positionDamper = 0;
    //        jointDrive.positionSpring = 0;
    //        joint.angularXDrive = jointDrive;
    //        joint.angularYZDrive = jointDrive;
    //    }
    //}

    /// <summary>
    /// Changes kinematic property of last wire point so it dangles like a wire
    /// or remains in place to be stationary or be controlled by the player.
    /// </summary>
    public void ChangeEndKinematic(bool isKine)
    {
        Rigidbody rb = _endTrans.GetComponent<Rigidbody>();
        rb.isKinematic = isKine;
    }

    /// <summary>
    /// Has the wire class generate a sphere over the segment to visualize it.
    /// </summary>
    /// <param name="parentObj">parent of the segment</param>
    /// <param name="isLastSegment">if it's the last wire segment</param>
    //private void GenerateSphereObj(Transform parentObj, bool isLastSegment)
    //{
    //    MGWire wireRef = GetComponentInParent<MGWire>();
    //    Assert.IsNotNull(wireRef);
    //    wireRef.CreateSegmentStruct(parentObj, isLastSegment);
    //}
}
