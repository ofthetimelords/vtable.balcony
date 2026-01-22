# My Balcony

Did you ever have a dream about something completely silly and then proceeded to make something out of it?

That's what happened here. I have an unused security camera and no places to put it (I live in an apartment).
So, while trying to figure out what to do with it, I came across fal.ai, which provides API access to many different models that provide text-to-image and image-to-image operations (among others).

After looking at their models a bit more extensively and dismissing most of them because of their results, I found flux-2/klein/4b/edit which appears to provide a good enough solution for what I need.
It's also one of the cheapest models to run, at about $0.015 per request.

## The "vision"
I mounted the camera on my balcony, trying to capture as much of the open space as it allows (before the construction completes and then the camera's location will have to change.
Then, I wanted to take that snapshot and alter it. Not create a completely new image out of it, not merely adjust hue and saturation. Passing a "theme" to it as a prompt (randomly chosen from a pool of themes) whenever the image refreshes would create a result that still resembles the original (usually), but also add a twist to it.

## How do I build this?
*Note*: The primary deployment method for this is to be containerised, using Docker or Kubernetes.

Modify the build-example.sh to include your own repository and tag. The script will build the latest image and deploy it to the Docker registry or your own registry.
You can also run 'dotnet build'.

## Environment options
*Note*: Reading from the configuration file is doable but currently pending. For now, I'll use the options as they would be configured using the Environment settings

The formatting of the options as environment variables is due to how ASP.net Core configuration expects them to be defined.

|Environment Variable					|Explanation|
---------------------------------------------------------------------
| Balcony__AppConfig__PrivilegedKey			| (string) - Use this key to perform two operations: Generate New Image, and Delete Existing Image.|
| Balcony__AppConfig__RefreshPeriodMinutes		| (int)	   - How often to generate a new image.|
| Balcony__AppConfig__ImagesToKeep			| (int)    - How many images should be stored on disk.|
| Balcony__AppConfig__ImagesToShow			| (int)    - How many images should be shown on the UI.|
| Balcony__AppConfig__ImagesPath			| (string) - An optional path to store the images under. If omitted, it will use the system's Temp path.|
| Balcony__AppConfig__ObfuscateSnapshots		| (bool)   - This will apply some light blurring and pixelation of the image, to alleviate privacy concerns.|
| Balcony__AppServices__Camera				| (string) - The name of the Camera service to use. Currently, only *Tapo* is supported.|
| Balcony__AppServices__Image				| (string) - The name of the image generation service to use. Currently, only Flux2_Klein_4b_Edit is supported.|
| Balcony__AppServices__Payload				| (string) - The name of the payload generation service to use. Currently, only *FluxPayLoad* is supported.|
| Balcony__Tapo__Credentials__User			| (string) - The camera account's username.|
| Balcony__Tapo__Credentials__Password			| (string) - The camera account's password.|
| Balcony__Tapo__Credentials__Endpoint			| (string) - The camera's endpoint; IP Address or Hostname.|
| Balcony__Tapo__FixDistortionStrength			| (float)  - You can leave this at 0 if you do not want any barrel distortion correction to be applied. Positive values will increase barrel distortion, negative ones will reduce it. For a typical 110 degree FOV Tapo Camera, *-0.06* works well.|
| Balcony__Tapo__CropTop				| (int)    - Amount of pixels to crop from the top of the camera snapshot.|
| Balcony__Tapo__CropRight				| (int)    - Amount of pixels to crop from the right of the camera snapshot.|
| Balcony__Tapo__CropBottom				| (int)    - Amount of pixels to crop from the bottom of the camera snapshot.|
| Balcony__Tapo__CropLeft				| (int)    - Amount of pixels to crop from the left of the camera snapshot.|
| Balcony__Flux2_Klein_4b_Edit__Key			| (string) - Your *fal.ai* API key.|
| Balcony__FluxPayload__PromptsFile			| (string) - Leave blank to use the default prompts, or map it to your own prompts.json file. Have a look at the included file to understant its structure.|
| Balcony__FluxPayload__InferenceSteps			| (int)    - How many steps should the model take. Usually *6* works well.|
| Balcony__FluxPayload__SafetyChecker			| (bool)   - You can enable or disable this. If you keep it enabled, some mild prompts will fail (e.g. "horror movie").|
| Balcony__FluxPayload__ThemesSkipCache			| (int)    - A FIFO cache of previously used prompts & themes that is used to avoid generating themes with recurring themes too often. Usually, half of your prompts file should work well enough and guarantee that at least that many images will be different. *Note:* This will be rewritten to increase randomness quality.|
| Balcony__FluxPayload__ImageSize__Width		| (int)    - The width of the desired generated image. It does not have to match the camera's snapshot width.
| Balcony__FluxPayload__ImageSize__Height		| (int)    - The height of the desired generated image. It does not have to match the camera's snapshot height.

## Operations
### Generate New Image

### Delete Existing Image

## How do I deploy this?
*Note*: The primary deployment method for this project is to be containerised, using Docker or Kubernetes.

### Before you start:
1. Make sure that your camera supports RTSP.
    1. It should also have "camera-credentials" created for it; these are different from the camera's cloud credentials and you don't need cloud access for those.
1. Configure the camera's credentials in your respective configuration file.
1. Create a fal.ai account, add some credits to it and create an API key.

### Docker
You can use the deployment-examples/docker-compose.yaml file. You should define a backing volume in case you want the generated images and camera snapshots to persist.

### Kubernetes
You can use the deployment-example/kubernetes.manifest.yaml file. This manifest does not define an Ingress and the backing Service is of type LoadBalancer which you may need to adjust.
