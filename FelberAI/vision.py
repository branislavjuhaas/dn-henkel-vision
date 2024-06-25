import os
from google.cloud import vision

# Register an environment variable with the path to the service account key file: key.json

os.environ['GOOGLE_APPLICATION_CREDENTIALS'] = 'key.json'

client = vision.ImageAnnotatorClient()
context = vision.ImageContext(language_hints=['sk'])

path = 'images/faults.png'

with open(path, 'rb') as f:
    image = f.read()

image = vision.Image(content=image)

response = client.document_text_detection(image=image, image_context=context)

print(response.full_text_annotation.text)