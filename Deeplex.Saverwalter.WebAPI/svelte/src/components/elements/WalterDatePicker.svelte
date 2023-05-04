<script lang="ts">
  import {
    DatePicker,
    DatePickerInput,
    DatePickerSkeleton
  } from 'carbon-components-svelte';

  import { convertDate } from '$WalterServices/utils';

  export let labelText: string;
  export let value: string | undefined = undefined;
  export let placeholder: string | undefined = undefined;
  export let disabled: boolean | undefined = false;
</script>

{#await value}
  <DatePickerSkeleton />
{:then x}
  <DatePicker value={convertDate(x)} dateFormat="d.m.Y" datePickerType="single">
    <DatePickerInput
      {disabled}
      style="width: 100%"
      type="text"
      {placeholder}
      {labelText}
    />
  </DatePicker>
{/await}
