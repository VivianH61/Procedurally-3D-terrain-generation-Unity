using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GerstnerWave : MonoBehaviour {
	//numWaves = 4
	public float Steepness=1;//value range:[0, 1]
	public Vector4 Amp=Vector4.one;//amplitude
	public Vector4 Length=Vector4.one;//waveLength
	public Vector4 Speed=Vector4.one;
	public Vector2 Dir1=new Vector2(0.615f, 1);
	public Vector2 Dir2=new Vector2(0.788f, 0.988f);
	public Vector2 Dir3=new Vector2(0.478f,0.937f);
	public Vector2 Dir4=new Vector2(0.154f, 0.71f);

	private static Vector4 Dx = Vector4.zero;
	private static Vector4 Dz = Vector4.zero;
	private static Vector4 W = Vector4.zero;//W=2*PI/waveLength
	private static Vector4 Q = Vector4.zero;//steepness
	private static Vector4 S = Vector4.zero;//move speed m/s
	private static Vector4 A = Vector4.zero;//amplitude

	public Renderer[] m_renderers;
	// Use this for initialization
	void Start () 
	{
		m_renderers = GetComponentsInChildren<Renderer> ();
		SetParams (m_renderers);
	}

	void SetParams(Renderer[] renderers)
	{
		if (null == renderers)
			return;
		Dir1.Normalize ();
		Dir2.Normalize ();
		Dir3.Normalize ();
		Dir4.Normalize ();

		W = new Vector4(2 * Mathf.PI/ Length.x, 2 * Mathf.PI/ Length.y, 2 * Mathf.PI/ Length.z, 2 * Mathf.PI/ Length.w) ;
		Dx = new Vector4 (Dir1.x, Dir2.x, Dir3.x, Dir4.x);
		Dz = new Vector4 (Dir1.y, Dir2.y, Dir3.y, Dir4.y);
		S = Speed;
		A = Amp;

		Q = new Vector4(Steepness/(W.x*A.x*4), Steepness/(W.y*A.y*4), Steepness/(W.z*A.z*4), Steepness/(W.w*A.w*4));

		for (int i = 0; i < renderers.Length; i++) {
//			for (int j = 0; j < renderers [i].materials.Length; j++) {
//				renderers [i].materials [j].SetVector ("_QA", new Vector4(Q.x*Amp.x, Q.y*Amp.y, Q.z*Amp.z, Q.w*Amp.w));
//				renderers [i].materials [j].SetVector ("_A", Amp);
//				renderers [i].materials [j].SetVector ("_Dx", Dx);
//				renderers [i].materials [j].SetVector ("_Dz", Dz);
//				renderers [i].materials [j].SetVector ("_S", Speed);
//				renderers [i].materials [j].SetVector ("_L", W);
//			}
			renderers [i].sharedMaterial.SetVector ("_QA", Mul(Q, A));
			renderers [i].sharedMaterial.SetVector ("_A", A);
			renderers [i].sharedMaterial.SetVector ("_Dx", Dx);
			renderers [i].sharedMaterial.SetVector ("_Dz", Dz);
			renderers [i].sharedMaterial.SetVector ("_S", S);
			renderers [i].sharedMaterial.SetVector ("_L", W);
		}
	}

	public static Vector4 Mul(Vector4 a, Vector4 b)
	{
		return new Vector4 (a.x*b.x, a.y*b.y, a.z*b.z, a.w*b.w);
	}
	public static Vector4 Sin(Vector4 x)
	{
		return new Vector4 (Mathf.Sin(x.x), Mathf.Sin(x.y), Mathf.Sin(x.z), Mathf.Sin(x.w));
	}
	public static Vector4 Cos(Vector4 x)
	{
		return new Vector4 (Mathf.Cos(x.x), Mathf.Cos(x.y), Mathf.Cos(x.z), Mathf.Cos(x.w));
	}

	public static Vector3 CalculateWaveDisplacementNormal(Vector3 worldPos, out Vector3 normal)
	{
		Vector3 pos = Vector3.zero;
		Vector4 phase = Dx * worldPos.x + Dz * worldPos.z + S * Time.time;
		Vector4 sinp = Vector4.zero, cosp = Vector4.zero;

		sinp = Sin (Mul(W, phase));
		cosp = Cos (Mul(W, phase));

		pos.x = Vector4.Dot (Mul(Q, Mul(A, Dx)), cosp);
		pos.z = Vector4.Dot (Mul(Q, Mul(A, Dz)), cosp);
		pos.y = Vector4.Dot (A, sinp);

		normal.x = -Vector4.Dot (Mul(W, A), Mul(Dx, cosp));
		normal.z = -Vector4.Dot (Mul(W, A), Mul(Dz, cosp));
		normal.y = 1-Vector4.Dot (Mul(Q, Mul(A, W)), sinp);

		normal.Normalize ();

		return pos;
	}
	public static Vector3 CalculateShipMovement(Vector3 worldPos)
	{
		Vector3 move = Vector3.zero;
		Vector4 phase = Dx * worldPos.x + Dz * worldPos.z + S * Time.time;
		Vector4 sinp = Vector4.zero, cosp = Vector4.zero;

		sinp = Sin (Mul(W, phase));
		cosp = Cos (Mul(W, phase));

		//displacement
		move.y = Vector4.Dot (A, sinp);
		//normal 
		move.x = -Vector4.Dot (Mul(W, A), Mul(Dx, cosp));
		move.z = -Vector4.Dot (Mul(W, A), Mul(Dz, cosp));

		return move;
	}
	
	// Update is called once per frame
	void Update () {
		//can be removed
		SetParams (m_renderers);
	}
}
