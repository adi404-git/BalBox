using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private float _duration  = 0f;
    private float _magnitude = 0f;
    private float _elapsed   = 0f;
    private Vector3 _originalLocalPos;
    private bool _shaking = false;

    void LateUpdate()
    {
        if (!_shaking) return;

        _elapsed += Time.deltaTime;

        if (_elapsed < _duration)
        {
            // Decay: shake intensity reduces linearly to zero
            float decay = 1f - (_elapsed / _duration);
            float mag   = _magnitude * decay;

            transform.localPosition = _originalLocalPos + Random.insideUnitSphere * mag;
        }
        else
        {
            transform.localPosition = _originalLocalPos;
            _shaking = false;
        }
    }
    public void Shake(float duration, float magnitude)
    {
        if (!_shaking || magnitude > _magnitude)
        {
            _originalLocalPos = Vector3.zero; 
            _duration         = duration;
            _magnitude        = magnitude;
            _elapsed          = 0f;
            _shaking          = true;
        }
    }
}
