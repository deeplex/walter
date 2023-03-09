<script lang="ts">
	import { onMount } from 'svelte';
	import PDFObject from 'pdfobject';

	export let src: string;

	onMount(() => {
		PDFObject.embed(src, '#pdf-container');
	});
</script>

<div style="height: 100vw" id="pdf-container" />
