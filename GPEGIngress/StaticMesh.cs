// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPEGIngress
{
	public class Mesh
	{
		public int VertexCount = 10;
		public int IndexCount = 10;
		public List<float> Vertices = new List<float>() { 1, 2, 3, 4, 5, 6 };
		public List<int> Indices = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
	}

	public class StaticMesh
	{
		public Int64 TimeStamp = DateTime.UtcNow.Ticks;
		public int LODCount = 2;
		public List<Mesh> RenderMeshes = new List<Mesh>();
		public Mesh CollisionMesh = new Mesh();
		public Mesh OcclusionMesh = new Mesh();

		public StaticMesh()
		{
			RenderMeshes.Add( new Mesh() );
		}
	}
}
