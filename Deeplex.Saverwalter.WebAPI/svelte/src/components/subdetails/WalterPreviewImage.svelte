<script lang="ts">
	import { ImageLoader } from 'carbon-components-svelte';
	import { onDestroy, onMount } from 'svelte';
	import type { WalterS3File } from '../../types/WalterS3File.type';

	export let file: WalterS3File;

	let src: string;

	onDestroy(() => {
		URL.revokeObjectURL(src);
	});

	onMount(() => {
		if (file.Blob) {
			src = URL.createObjectURL(file.Blob);
		}
		// TODO may want to handle !file.blob
	});
</script>

<ImageLoader {src} />
