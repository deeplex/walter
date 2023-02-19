<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { JuristischePersonEntry } from '../../../../types/juristischeperson.type';
	import { walter_get } from '../../../../services/utils';
	import {
		Kontakte,
		Person,
		WalterHeader,
		WalterGrid,
		WalterTextInput,
		Wohnungen,
		Vertraege,
		Anhaenge
	} from '../../../../components';
	import type { WohnungListEntry } from '../../../../types/wohnunglist.type';
	import type { VertragListEntry } from '../../../../types/vertraglist.type';

	export let data: PageData;
	const a: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);

	const w: Promise<WohnungListEntry[]> = walter_get(
		`/api/wohnungen/mieter/${data.id}`
	);
	const v: Promise<VertragListEntry[]> = walter_get(
		`/api/vertraege/mieter/${data.id}`
	);
</script>

<WalterHeader title={a.then((x) => x.name)}>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<Person person={a} />

	<Accordion>
		<Kontakte title="Mitglieder" rows={a.then((x) => x.mitglieder)} />
		<Kontakte
			title="Juristische Personen"
			rows={a.then((x) => x.juristischePersonen)}
		/>
		<Wohnungen title="Wohnungen" rows={w} />
		<Vertraege title="Verträge" rows={v} />
	</Accordion>
</WalterGrid>
