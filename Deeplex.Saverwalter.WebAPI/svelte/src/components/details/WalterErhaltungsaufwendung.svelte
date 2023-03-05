<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';

	export let a: Promise<Partial<WalterErhaltungsaufwendungEntry>> | undefined =
		undefined;
	export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<Row>
	<WalterTextInput
		bind:binding={entry.bezeichnung}
		labelText="Bezeichnung"
		value={a?.then((x) => x.bezeichnung)}
	/>
	<WalterComboBox
		bind:binding={entry.aussteller}
		titleText="Aussteller"
		api={'/api/selection/kontakte'}
		value={a?.then((x) => x.aussteller)}
	/>
	<WalterDatePicker
		bind:binding={entry.datum}
		value={a?.then((x) => x.datum)}
		labelText="Datum"
	/>
</Row>
<Row>
	<WalterComboBox
		bind:binding={entry.wohnung}
		titleText="Wohnung"
		value={a?.then((x) => x.wohnung)}
		api={'/api/selection/wohnungen'}
	/>
	<WalterNumberInput
		bind:binding={entry.betrag}
		label="Betrag"
		value={a?.then((x) => x.betrag)}
	/>
</Row>
<Row>
	<WalterTextInput
		bind:binding={entry.notiz}
		labelText="Notiz"
		value={a?.then((x) => x.notiz)}
	/>
</Row>
