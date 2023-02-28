<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { ErhaltungsaufwendungEntry } from '../../../types/erhaltungsaufwendung.type';
	import { walter_get } from '../../../services/utils';
	import {
		Anhaenge,
		SaveWalter,
		WalterComboBox,
		WalterDatePicker,
		WalterGrid,
		WalterHeader,
		WalterNumberInput,
		WalterTextInput
	} from '../../../components';

	export let data: PageData;
	const url = `/api/erhaltungsaufwendungen/${data.id}`;

	const a: Promise<ErhaltungsaufwendungEntry> = walter_get(url);
	const entry: Partial<ErhaltungsaufwendungEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader title={a.then((x) => x.aussteller.text + ' - ' + x.bezeichnung)}>
	<SaveWalter {a} {url} body={entry} />
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

<WalterGrid>
	<Row>
		<WalterTextInput
			bind:binding={entry.bezeichnung}
			labelText="Typ"
			value={a.then((x) => x.bezeichnung)}
		/>
		<WalterComboBox
			bind:binding={entry.aussteller}
			titleText="Aussteller"
			api={'/api/selection/kontakte'}
			value={a.then((x) => x.aussteller)}
		/>
		<WalterDatePicker value={a.then((x) => x.datum)} labelText="Datum" />
	</Row>
	<Row>
		<WalterComboBox
			bind:binding={entry.wohnung}
			titleText="Wohnung"
			value={a.then((x) => x.wohnung)}
			api={'/api/selection/wohnungen'}
		/>
		<WalterNumberInput
			bind:binding={entry.betrag}
			label="Betrag"
			value={a.then((x) => x.betrag)}
		/>
	</Row>
	<Row>
		<WalterTextInput
			bind:binding={entry.notiz}
			labelText="Notiz"
			value={a.then((x) => x.notiz)}
		/>
	</Row>
</WalterGrid>
