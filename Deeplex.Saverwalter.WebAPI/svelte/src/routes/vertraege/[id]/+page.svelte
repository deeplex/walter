<script lang="ts">
	import { Accordion, Button, Row, Truncate } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterKontakte,
		WalterMieten,
		WalterMietminderungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterVertrag,
		WalterVertragVersionen,
		WalterNumberInput
	} from '$WalterComponents';
	import type {
		WalterMieteEntry,
		WalterMietminderungEntry,
		WalterPersonEntry,
		WalterVertragVersionEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';
	import { create_abrechnung } from '$WalterServices/abrechnung';
	import {
		create_walter_s3_file_from_file,
		walter_s3_post
	} from '$WalterServices/s3';

	export let data: PageData;

	const today = new Date();
	const mietminderungEntry: Partial<WalterMietminderungEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		ende: toLocaleIsoString(new Date(today.setMonth(today.getMonth() + 1)))
	};

	const lastVersion = data.a.versionen
		? data.a.versionen[data.a.versionen?.length - 1]
		: undefined;
	const vertragversionEntry: Partial<WalterVertragVersionEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		personenzahl: lastVersion?.personenzahl,
		grundmiete: lastVersion?.grundmiete
	};

	const mieteEntry: Partial<WalterMieteEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		zahlungsdatum: toLocaleIsoString(new Date()),
		betrag: lastVersion?.grundmiete || 0
	};

	let jahr: number = new Date().getFullYear() - 1;

	function abrechnung_click(id: string, j: number) {
		create_abrechnung(id, j, title).then((e) => {
			const file = create_walter_s3_file_from_file(e, data.S3URL);
			walter_s3_post(new File([e], file.FileName), `${data.S3URL}`).then(
				(e) => {
					if (e.ok) {
						data.anhaenge = [...data.anhaenge, file];
					}
				}
			);
		});
	}

	const mieterEntry: Partial<WalterPersonEntry> = {};

	let title =
		data.a.wohnung?.text + ' - ' + data.a.mieter?.map((m) => m.name).join(', ');
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	bind:a={data.a}
	apiURL={data.apiURL}
	{title}
/>

<WalterGrid>
	<WalterVertrag
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		bind:a={data.a}
	/>

	<Accordion>
		<WalterKontakte
			a={mieterEntry}
			juristischePersonen={data.juristischePersonen}
			title="Mieter"
			rows={data.a.mieter}
		/>
		<WalterVertragVersionen
			a={vertragversionEntry}
			title="Versionen:"
			rows={data.a.versionen}
		/>
		<WalterMieten a={mieteEntry} title="Mieten" rows={data.a.mieten} />
		<WalterMietminderungen
			a={mietminderungEntry}
			title="Mietminderungen"
			rows={data.a.mietminderungen}
		/>
	</Accordion>

	<hr style="margin: 2em" />
	<Truncate>Betriebskostenabrechnung erstellen:</Truncate>

	<Row>
		<WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
		<Button size="small" on:click={() => abrechnung_click(data.id, jahr)}>
			Erstellen
		</Button>
	</Row>
</WalterGrid>
