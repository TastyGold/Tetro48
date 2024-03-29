﻿using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class BoardCamera
    {
        public Camera2D camera;

        private float targetAngle = 0;
        public float rotationSpeed = 360;

        public void Update()
        {
            float delta = rotationSpeed * Raylib.GetFrameTime();
            if (Math.Abs(camera.Rotation - targetAngle) > delta)
            {
                camera.Rotation += (delta) * Math.Sign(targetAngle - camera.Rotation);
            }
            else
            {
                camera.Rotation = targetAngle;
            }

            if (camera.Rotation < 0)
            {
                camera.Rotation += 360;
                targetAngle += 360;
            }
            if (camera.Rotation > 360)
            {
                camera.Rotation -= 360;
                targetAngle -= 360;
            }
        }

        public bool TargetAngleReached()
        {
            return camera.Rotation == targetAngle;
        }

        public VecInt2 GetMouseTile(int boardWidth, int boardHeight, int boardTileSize)
        {
            Vector2 mousePos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

            mousePos /= 8;
            mousePos.X += boardWidth / 2;
            mousePos.Y += boardHeight / 2;
            if (boardWidth % 2 != 0) mousePos.X += 0.5f;
            if (boardHeight % 2 != 0) mousePos.Y += 0.5f;

            int x = (int)mousePos.X;
            int y = (int)mousePos.Y;

            x = Math.Clamp(x, 0, boardWidth - 1);
            y = Math.Clamp(y, 0, boardHeight - 1);

            return new VecInt2(x, y);
        }

        public int GetMouseCell(int boardWidth, int boardHeight, int boardTileSize)
        {
            VecInt2 tile = GetMouseTile(boardWidth, boardHeight, boardTileSize);
            return tile.x + boardWidth * tile.y;
        }

        public void SetTargetAngle(float angle)
        {
            targetAngle = angle;
        }

        public float GetTargetAngle() => targetAngle;

        public void Rotate(bool clockwise)
        {
            SetTargetAngle(targetAngle + (clockwise ? 90 : -90));
        }
        public void Rotate180(bool clockwise)
        {
            SetTargetAngle(targetAngle + 180);
            camera.Rotation += clockwise ? 0.1f : -0.1f;
        }

        public BoardCamera(int screenScale)
        {
            camera = new Camera2D(new Vector2(100 * screenScale, 65 * screenScale), Vector2.Zero, 0, screenScale);
        }
    }
}