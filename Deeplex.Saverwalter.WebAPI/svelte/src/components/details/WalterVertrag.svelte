<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';

	export let a: Partial<WalterVertragEntry> = {};

	// TODO
	// let vermieter = () =>
	// 	kontakte.then(async (e) => {
	// 		const besitzer = await async.then((e) => e.wohnung.besitzerId);
	// 		return e.find((f) => besitzer === f.id)?.text;
	// 	});
</script>

<Row>
	<WalterDatePicker bind:value={a.beginn} labelText="Beginn" />
	<WalterDatePicker bind:value={a.ende} labelText="Ende" placeholder="Offen" />
</Row>
<Row>
	<WalterComboBox
		bind:value={a.wohnung}
		api={`/api/selection/wohnungen`}
		titleText="Wohnung"
	/>
	<!-- TODO -->
	<!-- <Column>
            <div style="margin-top:0.75rem">
                <p class="bx--label">Vermieter:</p>
                {#await vermieter()}
                    <TextInputSkeleton />
                {:then y}
                    <p style="margin-top: 0.5rem" class=".bx--text-input::placeholder">
                        {y}
                    </p>
                {/await}
            </div>
        </Column> -->
	<!-- TODO -->
	<WalterComboBox
		bind:value={a.ansprechpartner}
		api={`/api/selection/kontakte`}
		titleText="Ansprechpartner"
	/>
</Row>
<Row>
	<WalterMultiSelect
		bind:value={a.selectedMieter}
		api="/api/selection/kontakte"
		titleText="Mieter"
	/>
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
