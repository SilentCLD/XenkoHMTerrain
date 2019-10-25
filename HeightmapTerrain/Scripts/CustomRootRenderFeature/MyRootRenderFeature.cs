using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Streaming;
using HeightmapTerrain;

namespace HeightmapTerrain.Scripts.CustomRootRenderFeature
{
    public class MyRootRenderFeature : RootRenderFeature
    {
        private MutablePipelineState pipelineState;
        private DynamicEffectInstance myCustomShader;

        public override Type SupportedRenderObjectType => typeof(TerrainWrapper);

        public MyRootRenderFeature()
        {
            //pre adjust render priority, low numer is early, high number is late
            SortKey = 0;
        }

        protected override void InitializeCore()
        {
            // Initalize the shader
            myCustomShader = new DynamicEffectInstance("MyCustomShader");
            myCustomShader.Initialize(Context.Services);

            // Create the pipeline state and set properties that won't change
            pipelineState = new MutablePipelineState(Context.GraphicsDevice);
            pipelineState.State.SetDefaults();
            pipelineState.State.InputElements = TerrainWrapper.VertexDeclaration.CreateInputElements();
            pipelineState.State.PrimitiveType = TerrainWrapper.PrimitiveType;
            pipelineState.State.BlendState = BlendStates.Default;
            pipelineState.State.RasterizerState.CullMode = CullMode.None;
        }

        public override void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);

            // Register resources usage
            foreach (var renderObject in RenderObjects)
            {
                var myRenderObject = (TerrainWrapper)renderObject;
                Context.StreamingManager?.StreamResources(myRenderObject.Texture, StreamingOptions.LoadAtOnce);
                myRenderObject.Prepare(context);
            }
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            // First do everything that doesn't change per individual render object
            var graphicsDevice = context.GraphicsDevice;
            var graphicsContext = context.GraphicsContext;
            var commandList = context.GraphicsContext.CommandList;

            // Refresh shader, might have changed during runtime
            myCustomShader.UpdateEffect(graphicsDevice);

            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var myRenderObject = (TerrainWrapper)renderNode.RenderObject;

                if (myRenderObject.VertexBuffer == null)
                    continue; //next render object

                // Assign shader parameters
                myCustomShader.Parameters.Set(TransformationKeys.WorldViewProjection, myRenderObject.WorldMatrix * renderView.ViewProjection);
                myCustomShader.Parameters.Set(TexturingKeys.Texture0, myRenderObject.Texture);
                myCustomShader.Parameters.Set(MyCustomShaderKeys.TextureScale, myRenderObject.TextureScale);
                //myCustomShader.Parameters.Set(MyCustomShaderKeys.Color, myRenderObject.Color);

                // Prepare pipeline state
                pipelineState.State.RootSignature = myCustomShader.RootSignature;
                pipelineState.State.EffectBytecode = myCustomShader.Effect.Bytecode;
                pipelineState.State.Output.CaptureState(commandList);
                pipelineState.Update();
                commandList.SetPipelineState(pipelineState.CurrentState);

                // Apply the effect
                myCustomShader.Apply(graphicsContext);

                // Set vertex buffer and draw
                commandList.SetVertexBuffer(0, myRenderObject.VertexBuffer, 0, TerrainWrapper.VertexDeclaration.VertexStride);
                commandList.SetIndexBuffer(myRenderObject.IndexBuffer, 0, true);
                //commandList.Draw(myRenderObject.VertexBuffer.ElementCount, 0);
                commandList.DrawIndexed(myRenderObject.IndexBuffer.ElementCount);

            }
        }
    }
}
