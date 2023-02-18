import { writable, type Writable } from 'svelte/store';

export let isSideNavOpen: Writable<boolean> = writable(true);