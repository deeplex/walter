<script lang="ts">
	import { WalterPreviewPdf } from '$WalterComponents';
	import { download_file_blob, walter_s3_delete } from '$WalterServices/s3';
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
	export let blob: Blob | undefined = undefined;
	export let name: string;
	export let url: string;

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

	function handleModalClose() {
		URL.revokeObjectURL(url);
	}

	function download() {
		if (blob) {
			download_file_blob(blob, name);
		}
	}

	function remove() {
		console.log(blob);

		// 	if (blob) {
		// 		walter_s3_delete(`${url}/${name}`, name);
		// 	}
	}

	function close() {
		open = false;
	}

	let objectURL: string;
	function createObjectURL(blob: Blob) {
		objectURL = URL.createObjectURL(blob);
		console.log(objectURL);
		return url;
	}
</script>

<ComposedModal
	bind:open
	on:open={handleModalOpen}
	on:close={handleModalClose}
	on:submit
>
	<ModalHeader title={name} />
	<ModalBody>
		{#if blob}
			{#if blob.type === 'image/png'}
				<ImageLoader src={createObjectURL(blob)} />
			{:else if blob.type === 'text/plain'}
				<Tile light>{text}</Tile>
			{:else if blob.type === 'application/pdf'}
				<div style="height:100vw">
					<WalterPreviewPdf src={createObjectURL(blob)} />
				</div>
			{:else}
				<Tile light>
					Kann für die Datei: {name} keine Vorschau anzeigen. Dateityp: {blob.type}.
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
