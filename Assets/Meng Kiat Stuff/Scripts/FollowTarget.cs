using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform followTargetThirdPerson;
    [SerializeField] private Transform followTargetFirstPerson;

    [SerializeField] private float rotationalSpeed = 10f;
    [SerializeField] private float BottomClamp = -40f;
    [SerializeField] private float TopClamp = 70f;

    private float cinemachineTargetPitch;
    private float cinemachineTargetYaw;

    private void LateUpdate()
    {
        CameraLogic();
    }

    private void CameraLogic()
    {
        float mouseX = GetMouseInput("Mouse X");
        float mouseY = GetMouseInput("Mouse Y");

        cinemachineTargetPitch = UpdateRotation(cinemachineTargetPitch, mouseY, BottomClamp, TopClamp, true);
        cinemachineTargetYaw = UpdateRotation(cinemachineTargetYaw, mouseX, float.MinValue, float.MaxValue, false);

        ApplyRotations(cinemachineTargetPitch, cinemachineTargetYaw);
    }

    private void ApplyRotations(float pitch, float yaw)
    {
        followTargetThirdPerson.rotation = Quaternion.Euler(pitch, yaw, followTargetThirdPerson.eulerAngles.z);
        followTargetFirstPerson.rotation = Quaternion.Euler(pitch, yaw, followTargetFirstPerson.eulerAngles.z);
    }

    private float UpdateRotation(float currentRotation, float input, float min, float max, bool isXAxis)
    {
        currentRotation += isXAxis ? -input : input;
        return Mathf.Clamp(currentRotation, min, max);
    }


    private float GetMouseInput(string axis)
    {
        return Input.GetAxis(axis) * rotationalSpeed * Time.deltaTime;
    }
}
