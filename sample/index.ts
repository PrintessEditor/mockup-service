import { iPrintessApi } from "./printess-editor";

const shopAuthToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6InByaW50ZXNzLXNhYXMtYWxwaGEiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJneTh6NDRFbUxpWjB2clVLclhsV3RpWlkxNUQyIiwianRpIjoibnl4NGJFZlZ5SVVvM1NkR2dYVnEtempzS3hlb0FYZUYiLCJyb2xlIjoic2hvcCIsIm5iZiI6MTYxODg2ODUxMSwiZXhwIjoxOTM0MjI4NTExLCJpYXQiOjE2MTg4Njg1MTEsImlzcyI6IlByaW50ZXNzIEdtYkggJiBDby5LRyIsImF1ZCI6InByaW50ZXNzLXNhYXMifQ.CuI6zdCzBm2y3t2GBD4pYdyztFzSeSEfdGIDBeiZIYvzQckB9oEB1Z4hDkBCGZGBtTMRyuHCbkwZgO6uxD-ZyahifiqrIqfqSqtFEGwgZUF87TvV_KlrYWBzDNTaUIQjr-yUoxLkdnEMzh-3D5qV8UKWIDfqwnYd0KhJiB2K9CSg82_etnz5Lk-altMDAT8b1AnzxcjRAJ9_b6-CAJFXG6AAnfdl7c_PS3sD-RPOkJ75Ta2glfikIiGZzfh09bn5Ptk7rucRdxUsLCLR6m5nUFpZbV77d2eqRw8pT4Kl-5by5gvMr1wUBGbEx751CNXtcCO3qk4uNnptfZ3yCpK0Z2FOo2CYLZBzmDiYCrdFV5U-_SZuVOEl8vk0uR3tj_PQci_R7MlQOjpB4NjlKckQ2zGvBSKNeupuiC71UZ2AT5BFlbqMsuYu0necIztyKiWsBmbniVlLe-v7_paP1N4nS2haD2n4s4N_CenJqijtPggWsITfoLm2twCOe7yNB5IH7bcEFv1-MbANuaFmJVLOcTfc89Zi-mkidaHV-n_9qXypzyB-ih_27YBNluRGwcHgTEkbJecSssMfvHSt1MUuqX-8gbl7bhFGryqHA2gMoSZNDW0LkSYig2K3poOUumD67vtYdNSPLhOmDK4ck9wLAKLOvk6dtywg2qfV-58_VbI";

export interface IMockupData {
  templateName: string;
  documentName?: string;
}

export async function createMockupUrl(data: IMockupData, saveToken: string) {
  const response = await fetch("https://api.printess.com/saas/mockup/url/create", {
    method: "POST",
    mode: "cors",
    cache: "no-cache",
    credentials: "omit",
    headers: {
      "Content-Type": "application/json",
      "Authorization": "Bearer " + shopAuthToken
    },
    redirect: "follow",
    referrerPolicy: "no-referrer",
    body: JSON.stringify({
      templateName: data.templateName,
      documentName: data.documentName,
      contentTemplateName: saveToken
    })
  });

  if (!response.ok) {
    throw new Error(response.statusText);
  }

  const result = await response.json();
  
  return result.url;
}

export async function loadPrintess(templateName: string, addToBasketCallback: (token: string, thumbnailUrl: string) => void): Promise<iPrintessApi> {
  // @ts-ignore
  const printessLoader = await import("https://editor.printess.com/printess-editor/loader.js");

  const printess = await printessLoader.load({
    token: shopAuthToken,
    templateName: templateName,
    templateVersion: "draft",
    basketId: "Some-Unique-Basket-Or-Session-Id",
    addToBasketCallback,
    container: document.getElementById("printess-editor")
  });

  console.log(printess)

  return <iPrintessApi>printess.api;
}

export async function renderMockups(host: HTMLElement, templates: IMockupData[], saveToken: string, callback: (templateName: string, saveToken: string) => void) {
  for (const c of Array.from(host.children)) {
    c.remove();
  }

  for (const mockup of templates) {
    const div = document.createElement("div");
    const image = document.createElement("img");
    const text = document.createElement("div");
    const mockupUrl = await createMockupUrl(mockup, saveToken);

    image.src = mockupUrl;
    image.className = "mockup-image";

    text.textContent = mockup.templateName;
    text.className = "mockup-text";

    div.dataset["templateName"] = mockup.templateName;
    div.dataset["saveToken"] = saveToken;
    div.className = "mockup-div";
    
    div.appendChild(image);
    div.appendChild(text);

    div.addEventListener("click", function () {
      callback(this.dataset["templateName"] ?? "", this.dataset["saveToken"] ?? "")
    });
    host.appendChild(div);
  }
}