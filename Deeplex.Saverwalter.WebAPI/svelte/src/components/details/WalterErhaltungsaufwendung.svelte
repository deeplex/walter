<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';

	export let a: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<Row>
	<WalterTextInput bind:value={a.bezeichnung} labelText="Bezeichnung" />
	<WalterComboBox
		bind:value={a.aussteller}
		titleText="Aussteller"
		api={'/api/selection/kontakte'}
	/>
	<WalterDatePicker bind:value={a.datum} labelText="Datum" />
</Row>
<Row>
	<WalterComboBox
		bind:value={a.wohnung}
		titleText="Wohnung"
		api={'/api/selection/wohnungen'}
	/>
	<WalterNumberInput bind:value={a.betrag} label="Betrag" />
</Row>
<Row>
	<WalterTextInput bind:value={a.notiz} labelText="Notiz" />
</Row>
