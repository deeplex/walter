<script lang="ts">
	import {
		WalterPreviewImage,
		WalterPreviewPdf,
		WalterPreviewText
	} from '$WalterComponents';
	import { download_file_blob, walter_s3_delete } from '$WalterServices/s3';
	import type { WalterS3File } from '$WalterTypes';
	import {
		Button,
		ComposedModal,
		ModalBody,
		ModalFooter,
		ModalHeader,
		Tile
	} from 'carbon-components-svelte';

	export let open: boolean = false;
	export let file: WalterS3File;

	function download() {
		if (file.Blob) {
			download_file_blob(file.Blob, file.FileName);
		}
	}

	function remove() {
		walter_s3_delete(file, '');
	}
</script>

<ComposedModal bind:open on:submit>
	<ModalHeader bind:title={file.FileName} />
	<ModalBody>
		{#if file.Type === 'image/png'}
			<WalterPreviewImage {file} />
		{:else if file.Type === 'text/plain' || file.Type === 'application/json'}
			<WalterPreviewText {file} />
		{:else if file.Type === 'application/pdf'}
			<WalterPreviewPdf {file} />
		{:else}
			<Tile light>
				Kann für die Datei: {file.FileName} keine Vorschau anzeigen. Dateityp:
				{file.Type}.
			</Tile>
		{/if}
	</ModalBody>
	<ModalFooter>
		<Button kind="secondary" on:click={close}>Abbrechen</Button>
		<Button kind="danger" on:click={remove}>Löschen</Button>
		<Button kind="primary" on:click={download}>Herunterladen</Button>
	</ModalFooter>
</ComposedModal>
