// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPEGIngress
{
	public enum EActorType
    {
		Light,
		SkeletalMesh,
		StaticMesh,
		Brush
	}

	public class SceneActor
	{
		public string Actor = "SM_Actor";
		public EActorType ActorType = EActorType.StaticMesh;
		public float[] Origin = new float[3] { 0.0f, 0.0f, 0.0f };
		public float[] Extents = new float[3] { 0.0f, 0.0f, 0.0f };
	}

	public class Scene
	{
		public List<SceneActor> SceneActors = new List<SceneActor>();

		public Scene()
		{
			SceneActors.Add( new SceneActor() );
			SceneActors.Add( new SceneActor() );
		}
	}
}
