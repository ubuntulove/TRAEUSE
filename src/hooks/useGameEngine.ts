
import { useEffect, useRef, useState } from "react";
import { CanvasRenderer } from "../engine/CanvasRenderer";
import { SceneManager } from "../engine/SceneManager";
import { PhysicsSystem } from "../engine/PhysicsSystem";
import { InputHandler } from "../engine/InputHandler";
import { GameObject } from "../engine/GameObject";

export function useGameEngine() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rendererRef = useRef<CanvasRenderer | null>(null);
  const [isRunning, setIsRunning] = useState(false);

  useEffect(() => {
    if (!canvasRef.current) return;

    const sceneManager = new SceneManager();
    const physicsSystem = new PhysicsSystem();
    const inputHandler = new InputHandler();
    const renderer = new CanvasRenderer(
      canvasRef.current,
      sceneManager,
      physicsSystem,
      inputHandler
    );

    // Add some demo objects
    const createDemoObjects = () => {
      // Add a player
      const player = new GameObject(
        "player",
        { x: 100, y: 100 },
        { x: 50, y: 50 },
        "#00d4ff"
      );
      // Override update for player (keyboard control)
      player.update = (deltaTime: number) => {
        const speed = 300;
        if (inputHandler.isKeyDown("ArrowLeft") || inputHandler.isKeyDown("KeyA")) {
          player.velocity.x -= speed * deltaTime * 10;
        }
        if (inputHandler.isKeyDown("ArrowRight") || inputHandler.isKeyDown("KeyD")) {
          player.velocity.x += speed * deltaTime * 10;
        }
        if (inputHandler.isKeyDown("ArrowUp") || inputHandler.isKeyDown("KeyW")) {
          player.velocity.y -= speed * deltaTime * 10;
        }
        if (inputHandler.isKeyDown("ArrowDown") || inputHandler.isKeyDown("KeyS")) {
          player.velocity.y += speed * deltaTime * 10;
        }
        // Boundary check
        const canvas = canvasRef.current!;
        if (player.position.x < 0) {
          player.position.x = 0;
          player.velocity.x *= -0.5;
        }
        if (player.position.x + player.size.x > canvas.width) {
          player.position.x = canvas.width - player.size.x;
          player.velocity.x *= -0.5;
        }
        if (player.position.y < 0) {
          player.position.y = 0;
          player.velocity.y *= -0.5;
        }
        if (player.position.y + player.size.y > canvas.height) {
          player.position.y = canvas.height - player.size.y;
          player.velocity.y *= -0.5;
        }
      };
      sceneManager.addObject(player);

      // Add some random boxes
      for (let i = 0; i < 5; i++) {
        const box = new GameObject(
          `box-${i}`,
          {
            x: Math.random() * (canvasRef.current!.width - 100) + 50,
            y: Math.random() * (canvasRef.current!.height - 100) + 50,
          },
          { x: 40, y: 40 },
          `hsl(${Math.random() * 360}, 70%, 60%)`
        );
        sceneManager.addObject(box);
      }
    };

    createDemoObjects();
    rendererRef.current = renderer;

    // Initial render
    const ctx = canvasRef.current.getContext("2d")!;
    ctx.fillStyle = "#0a0e27";
    ctx.fillRect(0, 0, canvasRef.current.width, canvasRef.current.height);
    sceneManager.render(ctx);

    return () => {
      renderer.stop();
    };
  }, []);

  const handleStart = () => {
    rendererRef.current?.start();
    setIsRunning(true);
  };

  const handleStop = () => {
    rendererRef.current?.stop();
    setIsRunning(false);
  };

  const handleReset = () => {
    rendererRef.current?.reset();
    setIsRunning(false);
    // Recreate demo objects
    if (canvasRef.current && rendererRef.current) {
      const sceneManager = new SceneManager();
      // Reuse existing renderer's other systems
      const physicsSystem = rendererRef.current["physicsSystem"] as PhysicsSystem;
      const inputHandler = rendererRef.current["inputHandler"] as InputHandler;
      // Create new renderer with new scene manager
      const renderer = new CanvasRenderer(
        canvasRef.current,
        sceneManager,
        physicsSystem,
        inputHandler
      );
      rendererRef.current = renderer;
      // Add demo objects again
      const createDemoObjects = () => {
        const player = new GameObject(
          "player",
          { x: 100, y: 100 },
          { x: 50, y: 50 },
          "#00d4ff"
        );
        player.update = (deltaTime: number) => {
          const speed = 300;
          if (inputHandler.isKeyDown("ArrowLeft") || inputHandler.isKeyDown("KeyA")) {
            player.velocity.x -= speed * deltaTime * 10;
          }
          if (inputHandler.isKeyDown("ArrowRight") || inputHandler.isKeyDown("KeyD")) {
            player.velocity.x += speed * deltaTime * 10;
          }
          if (inputHandler.isKeyDown("ArrowUp") || inputHandler.isKeyDown("KeyW")) {
            player.velocity.y -= speed * deltaTime * 10;
          }
          if (inputHandler.isKeyDown("ArrowDown") || inputHandler.isKeyDown("KeyS")) {
            player.velocity.y += speed * deltaTime * 10;
          }
          const canvas = canvasRef.current!;
          if (player.position.x < 0) {
            player.position.x = 0;
            player.velocity.x *= -0.5;
          }
          if (player.position.x + player.size.x > canvas.width) {
            player.position.x = canvas.width - player.size.x;
            player.velocity.x *= -0.5;
          }
          if (player.position.y < 0) {
            player.position.y = 0;
            player.velocity.y *= -0.5;
          }
          if (player.position.y + player.size.y > canvas.height) {
            player.position.y = canvas.height - player.size.y;
            player.velocity.y *= -0.5;
          }
        };
        sceneManager.addObject(player);
        for (let i = 0; i < 5; i++) {
          const box = new GameObject(
            `box-${i}`,
            {
              x: Math.random() * (canvasRef.current!.width - 100) + 50,
              y: Math.random() * (canvasRef.current!.height - 100) + 50,
            },
            { x: 40, y: 40 },
            `hsl(${Math.random() * 360}, 70%, 60%)`
          );
          sceneManager.addObject(box);
        }
      };
      createDemoObjects();
      const ctx = canvasRef.current.getContext("2d")!;
      ctx.fillStyle = "#0a0e27";
      ctx.fillRect(0, 0, canvasRef.current.width, canvasRef.current.height);
      sceneManager.render(ctx);
    }
  };

  return { canvasRef, isRunning, handleStart, handleStop, handleReset };
}
