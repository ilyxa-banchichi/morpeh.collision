using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class PlayerEntity : TestEntity
    {
        [SerializeField]
        private float _jumpForce;

        protected void Awake()
        {
            base.Awake();
            entity.AddComponent<PlayerTag>();
        }
        
        protected void Update()
        {
            var deltaTime = Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.Space))
                _jumpForce = 3f;

            if (Input.GetKey(KeyCode.W))
                transform.Translate(deltaTime * Vector3.forward * 3f);
            if (Input.GetKey(KeyCode.S))
                transform.Translate(deltaTime * Vector3.back * 3f);
            if (Input.GetKey(KeyCode.A))
                transform.Translate(deltaTime * Vector3.left * 3f);
            if (Input.GetKey(KeyCode.D))
                transform.Translate(deltaTime * Vector3.right * 3f);
            
            if (_jumpForce > 0f)
            {
                _jumpForce -= deltaTime;
                _jumpForce = math.clamp(_jumpForce, 0f, float.MaxValue);
            }
            
            //transform.Translate(_jumpForce * deltaTime * Vector3.up + deltaTime * Vector3.down);

            ref var events = ref entity.GetComponent<CollisionEventsComponent>();
            foreach (var e in events.OnCollisionEnter)
            {
                Debug.Log("Enter");
            }
            foreach (var e in events.OnCollisionStay)
            {
                Debug.Log("Stay");
            }
            foreach (var e in events.OnCollisionExit)
            {
                Debug.Log("Exit");
            }
            
            base.Update();
        }
    }
}