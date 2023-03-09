<script lang="ts">
	import { WalterPreviewPdf } from '$WalterComponents';
	import { download_file_blob } from '$WalterServices/s3';
	import { ImageLoader, Modal, Tile } from 'carbon-components-svelte';

	export let open: boolean = false;
	export let blob: Blob | undefined = undefined;
	export let name: string;

	let text: string = '';

	function handleModalOpen() {
		if (blob && blob.type === 'text/plain') {
			const reader = new FileReader();
			reader.onload = function (event) {
				text = (event.target?.result as string) || '';
			};
			reader.readAsText(blob);
		}
	}

	function download() {
		if (blob) {
			download_file_blob(blob, name);
		}
	}
</script>

<Modal
	bind:open
	bind:modalHeading={name}
	primaryButtonText="Herunterladen"
	secondaryButtonText="Abbrechen"
	on:click:button--primary={download}
	on:click:button--secondary={() => (open = false)}
	on:open={handleModalOpen}
	on:close
	on:submit
>
	{#if blob}
		{#if blob.type === 'image/png'}
			<ImageLoader src={URL.createObjectURL(blob)} />
		{:else if blob.type === 'text/plain'}
			<Tile light>{text}</Tile>
		{:else if blob.type === 'application/pdf'}
			<div style="height:100vw">
				<WalterPreviewPdf src={URL.createObjectURL(blob)} />
			</div>
		{:else}
			<Tile>
				Kann für die Datei: {blob.name} keine Vorschau anzeigen. Dateityp: {blob.type}.
			</Tile>
		{/if}
	{/if}
</Modal>
