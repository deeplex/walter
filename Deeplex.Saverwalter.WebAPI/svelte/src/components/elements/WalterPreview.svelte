<script lang="ts">
	import { WalterPreviewPdf } from '$WalterComponents';
	import type { WalterS3File } from '$WalterTypes';
	import {
		Button,
		ComposedModal,
		ImageLoader,
		ModalBody,
		ModalFooter,
		ModalHeader,
		Tile
	} from 'carbon-components-svelte';

	export let open: boolean = false;
	export let file: WalterS3File;

	let text: string = '';
	function handleModalOpen() {
		if (file.Blob && file.Blob.type === 'text/plain') {
			const reader = new FileReader();
			reader.onload = function (event) {
				text = (event.target?.result as string) || '';
			};
			reader.readAsText(file.Blob);
		}
	}

	function handleModalClose() {
		URL.revokeObjectURL(objectURL);
	}

	function download() {
		if (file.Blob) {
			download_file_blob(file.Blob, file.FileName);
		}
	}

	function remove() {
		console.log(name);
	}

	let objectURL: string;
	function createObjectURL(blob: Blob): string {
		objectURL = URL.createObjectURL(blob);

		return objectURL;
	}
</script>

<ComposedModal
	bind:open
	on:open={handleModalOpen}
	on:close={handleModalClose}
	on:submit
>
	<ModalHeader bind:title={file.FileName} />
	<ModalBody>
		{#if file.Blob}
			{#if file.Type === 'image/png'}
				<ImageLoader src={createObjectURL(file.Blob)} />
			{:else if file.Type === 'text/plain' || file.Blob.type === 'application/json'}
				<Tile light>{text}</Tile>
			{:else if file.Type === 'application/pdf'}
				<div style="height:100vw">
					<WalterPreviewPdf src={createObjectURL(file.Blob)} />
				</div>
			{:else}
				<Tile light>
					Kann für die Datei: {name} keine Vorschau anzeigen. Dateityp: {file.Type}.
				</Tile>
			{/if}
		{/if}
	</ModalBody>
	<ModalFooter>
		<Button kind="secondary" on:click={close}>Abbrechen</Button>
		<Button kind="danger" on:click={remove}>Löschen</Button>
		<Button kind="primary" on:click={download}>Herunterladen</Button>
	</ModalFooter>
</ComposedModal>
