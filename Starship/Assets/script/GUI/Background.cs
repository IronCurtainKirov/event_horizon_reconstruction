using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class Background : MonoBehaviour
{
	public Color Color = Color.white;
	public float ColorBurst = 0.5f;

	void Start()
	{
		_mainCamera = Camera.main;
		_transform = transform;
		_renderer = GetComponent<SpriteRenderer>();
		var sprite = _renderer.sprite;
		var pixelsPerUnit = sprite.pixelsPerUnit;
		var width = sprite.texture.width;
		var height = sprite.texture.height;
		var screenRatio = Screen.width / Screen.height;
        var spriteRatio = width / height;
        if (screenRatio >= spriteRatio)
        {
            _scale = 2 * _mainCamera.orthographicSize * _mainCamera.aspect;
            _scaleVector = new Vector3(pixelsPerUnit / width, pixelsPerUnit / width, 1.0f);
        }
		else
        {
            _scale = 2 * _mainCamera.orthographicSize;
            _scaleVector = new Vector3(pixelsPerUnit / height, pixelsPerUnit / height, 1.0f);
        }
        _transform.localScale = _scaleVector * _scale;
    }

	void LateUpdate()
	{
		var alpha = 0.75f + ColorBurst * (1 + Mathf.Pow(Mathf.Sin(Time.time / 5), 5));        
		_renderer.material.color = new Color(alpha, alpha, alpha, 1);
	}

	private Vector2 _scaleVector;
	private float _scale;
	private Camera _mainCamera;
	private Transform _transform;
	private SpriteRenderer _renderer;
}
