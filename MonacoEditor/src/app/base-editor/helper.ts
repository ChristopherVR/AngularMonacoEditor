import { editor } from 'monaco-editor';
import { MonacoEditorConfig } from '../monaco-editor/config';
import { ITextModel, MonacoEditorOptions } from './types';

export const getBaseOptions = (options: MonacoEditorOptions): editor.IStandaloneEditorConstructionOptions => ({
  ...options,
  model: cast<ITextModel>(options.model),
});

type Config = Window & {
  require: {
    config(paths: unknown): void;
  };
} & typeof globalThis;

type RequireFunc = Window & {
  require(data: string[], cb: () => void): void;
} & typeof globalThis;

type Require = Window & {
  require?: unknown;
} & typeof globalThis;

/**Registers the AMD loader required for the monaco editor. */
export const registerAmdLoader = async (
  baseUrl: string,
  config: MonacoEditorConfig,
  options: MonacoEditorOptions,
  registerEventListeners: () => void,
  initMonaco: (options: MonacoEditorOptions) => void,
  resolve: () => Promise<void> | void,
) => {
  const amdLoaderCallback: () => void = () => {
    // Load monaco
    cast<Config>(window).require.config({ paths: { vs: `${baseUrl}` } });
    cast<RequireFunc>(window).require([`vs/editor/editor.main`], async () => {
      registerEventListeners();
      if (typeof config.onMonacoLoad === 'function') {
        config.onMonacoLoad();
      }
      initMonaco(options);
      await resolve();
    });
  };

  // Load AMD loader if necessary
  if (!cast<Require>(window).require) {
    const loaderScript: HTMLScriptElement = document.createElement('script');
    loaderScript.type = 'text/javascript';
    loaderScript.src = `${baseUrl}/loader.js`;
    loaderScript.addEventListener('load', amdLoaderCallback);
    document.body.appendChild(loaderScript);
  } else {
    amdLoaderCallback();
  }
};

const cast = <T>(payload: unknown) => payload as T;
