using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangulator2D : MonoBehaviour
{
  List<Vector3 > _all = new List<Vector3>();
  List<Vector3 > _unprocessed = new List<Vector3>();
  List<Vector3> _hull = new List<Vector3>();
  List<Vector3> _candidates = new List<Vector3>();
  private int _current;

  private List<int> _triangles = new List<int>();

  private float _rMin;
  private float _rMax;

  void OnEnable()
  {
    _unprocessed.Clear();
    _hull.Clear();
    _all.Clear();
    _triangles.Clear();

    for (int i = 0; i < 100; i++)
    {
      var w = 5;
      _all.Add(new Vector3(Random.value, Random.value)*w*2 - new Vector3(w, w));;
    }

    _all.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));
    _unprocessed = _all.ToList();
    _hull.AddRange(_all.Take(3));

    float[] arr = new float[3]
                  {
                    Vector3.Distance(_all[0], _all[1]),
                    Vector3.Distance(_all[1], _all[2]),
                    Vector3.Distance(_all[2], _all[0])
                  };
    _rMin = Mathf.Min(arr);
    _rMax = Mathf.Max(arr);

    _current = 3;
    _triangles.AddRange(new[] {0,1,2});
    _unprocessed.RemoveRange(0, 4);

    _candidates = _hull.Where(x => (x - _all[_current]).magnitude < _rMin).ToList();

    StartCoroutine(Triangulate());
  }

  private IEnumerator Triangulate()
  {
    yield return null;
  }

  void OnDrawGizmos()
  {
    var color = Gizmos.color;

    foreach (var point in _hull)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(point, 0.1f);
    }

    foreach (var point in _unprocessed)
    {
      Gizmos.color = Color.grey;
      Gizmos.DrawSphere(point, 0.1f);
    }

    for (int index = 0; index < _triangles.Count; index+=3)
    {
      Debug.DrawLine(_all[index+0], _all[index+1], Color.blue);
      Debug.DrawLine(_all[index+1], _all[index+2], Color.blue);
      Debug.DrawLine(_all[index+2], _all[index+0], Color.blue);
    }

    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(_all[_current], 0.1f);
    Gizmos.DrawWireSphere(_all[_current], _rMin);
    Gizmos.DrawWireSphere(_all[_current], _rMax);

    Gizmos.color = color;
  }
}
