using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoint : MonoBehaviour
{
    // the number of ring
    public int RING_NUM = 4;
    // the number of ray
    public int RAY_NUM = 10;

    // random number generator
    private System.Random random = new System.Random();

    private float[] base_theta;
    private float[] base_radius;

    public RandomPoint()
    {
        int seed = DateTime.Now.Millisecond;
        System.Random random = new System.Random(seed);
        //UnityEngine.Random.seed = DateTime.Now.Millisecond;

        RING_NUM = random.Next(3, 4);
        RAY_NUM = random.Next(9, 11);

        base_theta = new float[RAY_NUM];

        float step = 1.0f / RAY_NUM;
        float theta_offset = step * (float)random.NextDouble();

        for (uint i = 0; i < RAY_NUM; ++i)
        {
            base_theta[i] = randomNormal(i * step, 0.0004f) + theta_offset;
        }

        base_radius = new float[RING_NUM + 1];
        base_radius[0] = 0.0f;

        float centrifugation = 0.5f;
        float sum = 0.0f;

        for (uint i = 1; i <= RING_NUM; ++i)
        {
            base_radius[i] = base_radius[i - 1] + 1 + i * centrifugation;
        }

        for (uint i = 0; i <= RING_NUM; ++i)
        {
            base_radius[i] /= base_radius[RING_NUM];
            base_radius[i] += randomNormal(0.0f, 0.04f);
        }
    }

    public float randomNormal(float mean, float stdDev)
    {
        float u1 = 1.0f - (float)random.NextDouble();
        float u2 = 1.0f - (float)random.NextDouble();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

        return mean + stdDev * randStdNormal;
    }

    public Vector2 polarToCartesian(float r, float theta)
    {
        float rad = theta * Mathf.PI / 180.0f;
        return new Vector2(r * Mathf.Cos(rad), r * Mathf.Sin(rad));
    }

    public Vector2[] getRandomPoint(Vector2 center)
    {
        Vector2[] point = new Vector2[RING_NUM * RAY_NUM];

        for (uint i = 0; i < RING_NUM; ++i)
        {
            for (uint j = 0; j < RAY_NUM; ++j)
            {
                float r = base_radius[i];
                base_theta[j] += randomNormal(0.0f, 0.02f * r);
                float theta = base_theta[j] * 360;

                point[i * RAY_NUM + j] = polarToCartesian(r, theta) + center;
            }
        }

        return point;
    }
}
