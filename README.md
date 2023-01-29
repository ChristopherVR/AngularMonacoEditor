<p align="center"> 
  <img src="https://github.com/devicons/devicon/blob/master/icons/angularjs/angularjs-plain.svg" alt="Angular Logo" width="80px" height="80px">
</p>
<h1 align="center"> Angular Monaco Editor </h1>
<h3 align="center"> A custom monaco editor built in the Angular framework using TypeScript and the latest conventions. This editor is to demonstrate how one can use a Monaco editor in Angular as well as catering for cases like Material design and server intellisense. </h3>  
<h4 align="center"> Note: This project is still a WIP </h4>  
</br>

<!-- TABLE OF CONTENTS -->
<h2 id="table-of-contents"> :book: Table of Contents</h2>

<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project"> ➤ About The Project</a></li>
    <li><a href="#prerequisites"> ➤ Prerequisites</a></li>
    <li><a href="#setup"> ➤ Setup</a></li>
  </ol>
</details>

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

<!-- ABOUT THE PROJECT -->
<h2 id="about-the-project"> :pencil: About The Project</h2>

<p align="justify"> 
  This project aims to make it easier to use a monaco editor in Angular without relying on NGX. This editor will also support rendering multiple editors on the same page and providing C# intellisense using a .NET 7 backend framework.
</p>

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

<!-- PREREQUISITES -->
<h2 id="prerequisites"> :fork_and_knife: Prerequisites</h2>

[![made-with-angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/) <br>
[![Made with-dot-net](https://img.shields.io/badge/-Made%20with%20.NET-purple)](https://dotnet.microsoft.com/en-us/) <br>
[![build status][buildstatus-image]][buildstatus-url]

[buildstatus-image]: https://github.com/ChristopherVR/MovieSystem-React-DDD-Example/blob/main/.github/workflows/badge.svg
[buildstatus-url]: https://github.com/ChristopherVR/MovieSystem-React-DDD-Example/actions

<!--This project is written mainly in C# and JavaScript programming languages. <br>-->
The following open source packages are used in this project:
* <a href="https://material.angular.io/"> Angular Material UI Design</a> 
* <a href="https://angular.io/"> Angular</a> 
* <a href="https://microsoft.github.io/monaco-editor/"> Monaco Editor</a> 
* <a href="https://github.com/atularen/ngx-monaco-editor"> MaterialHQ NGX Monaco Editor</a> 
* <a href="https://github.com/materiahq/ngx-monaco-editor"> Atularen NGX Monaco Editor</a> 
 
![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

<!-- SETUP -->
<h2 id="setup"> :floppy_disk: Setup</h2>

<h3>1. Setup the monaco editor</h3>
Setup the AppModule to include the BaseEditorComponent & your custom editor component (*MonacoEditorComponent* in this case as a demo)

```
@NgModule({
  declarations: [AppComponent, BaseEditorComponent, MonacoEditorComponent],
  imports: [BrowserModule, AppRoutingModule, BrowserAnimationsModule],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
```

Render the base editor onto your custom component' HTML template

```
<div>
  <app-base-editor class="w-100 h-100 monaco-editor position-relative" [options]="editorOptions"></app-base-editor>
</div>
```

Pass the editor options object to indicate what language and settings should be enabled for the monaco editor

```
  editorOptions: MonacoEditorOptions = {
    theme: 'vs',
    language: 'csharp',
    // disables the dropdown menu when right-clicking
    contextmenu: false,
    model: {
      language: 'csharp',
    },
  };
```
<h3>2. Configure the API URL</h3>

Create an `environment.ts` file in the `environment` folder. Include the apiEndpoint property

```
export const environment = {
  production: false,
  apiEndpoint: 'https://localhost:5023/',
};
```

<h3>3. Configure the .NET Backend</h3>
See the `DynamicModuleController.cs` or the gRPC service `DynamicServiceV1.cs` as examples on how to use the IntellisenseServer project.

</p>

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

