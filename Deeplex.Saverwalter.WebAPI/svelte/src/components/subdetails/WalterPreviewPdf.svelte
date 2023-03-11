<script lang="ts">
	import { onDestroy, onMount } from 'svelte';
	import PDFObject from 'pdfobject';
	import type { WalterS3File } from '$WalterTypes';

	export let file: WalterS3File;

	let src: string;
	onDestroy(() => {
		URL.revokeObjectURL(src);
	});

	onMount(() => {
		if (file.Blob) {
			src = URL.createObjectURL(file.Blob);
			PDFObject.embed(src, '#pdf-container');
		}
		// TODO may want to handle !src.blob
	});
</script>

<div style="height:100vw">
	<div style="height: 100vw" id="pdf-container" />
</div>
